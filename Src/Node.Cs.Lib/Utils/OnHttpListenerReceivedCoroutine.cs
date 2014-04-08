// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using ConcurrencyHelpers.Coroutines;
using NetworkHelpers.Http;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Lib.Utils
{
	public class OnHttpListenerReceivedCoroutine : Coroutine
	{
		private HttpCoroutineListenerClient _listener;
		private NodeCsContext _context;
		private IResourceHandler _resourceHandler;
		private CultureInfo _listenerCulture;

		protected virtual Step InitializeContext(NodeCsContext context)
		{
			return InvokeAsTaskAndWait(() => context.Initialize(_listener.Context,
				GlobalVars.SessionStorage.SessionTimeoutSeconds,
				GlobalVars.SessionStorage.RunId));
		}

		protected virtual NodeCsContext AssignContext(NodeCsContext context)
		{
			return context;
		}

		protected virtual bool IsChildRequest { get { return false; } }

		public override IEnumerable<Step> Run()
		{
			InitializeCulture();
			var context = new NodeCsContext();
			yield return InitializeContext(context);

			_context = AssignContext(context);
			CheckException();



			// ReSharper disable once UnusedVariable
			var response = _context.Response;
			var request = _context.Request;
			// ReSharper disable once UnusedVariable
			var url = request.Url;
			// ReSharper disable once PossibleNullReferenceException
			var localPath = url.LocalPath.Trim();
			var routeDefintion = GlobalVars.RoutingService.Resolve(localPath, _context);
			//var somethingHappened = false;
			if (routeDefintion != null)
			{
				_context.RouteParams = routeDefintion.Parameters ?? new Dictionary<string, object>();
			}

			if (_context.Session == null) throw new NullReferenceException("Missing session.");
			var session = (NodeCsHttpSession)_context.Session;
			//Retrieve the storage SessionID
			var storedSid = session.SessionID.Substring(1);
			var actionResult = new Container();

			if (!IsChildRequest)
			{
				//if (session.SessionID.StartsWith(NodeCsContext.SESSION_FULL.ToString()))
				{
					yield return InvokeLocalAndWait(() => GlobalVars.SessionStorage.CreateSession(storedSid), actionResult);
				}
				var dataDescriptor = actionResult.RawData as Dictionary<string, object>;
				if (dataDescriptor == null)
				{
					dataDescriptor = new Dictionary<string, object>();
				}

				if (dataDescriptor != null)
				{
					foreach (var kvp in dataDescriptor)
					{
						session.SessionData.Add(kvp.Key, kvp.Value);
					}
				}
				yield return Step.Current;
				CheckException();
			}

			if (IsNotStaticRoute(routeDefintion))
			{
				foreach (var filter in GlobalVars.GlobalFilters)
				{
					filter.OnPreExecute(_context);
				}
				var resultDate = new Container();
				yield return InvokeControllerAndWait(() =>
					NodeCsServer.Instance.HandleRoutedRequests(routeDefintion, localPath, _context, IsChildRequest),
						resultDate);

				CheckException();

				var result = resultDate.RawData as IResponse;
				if (result == null)
				{
					StoreContext(_context);
					yield break;
				}
				if (result != null)
				{
					foreach (var filter in GlobalVars.GlobalFilters)
					{
						filter.OnPostExecute(_context, result);
					}
					if (result is ViewResponse)
					{
						//Its responsability to close connection
						var viewResponse = result as ViewResponse;
						ViewData = viewResponse.ViewData;
						yield return HandleResult(result, _context, viewResponse.ViewBag, viewResponse.ModelState);
						yield return Step.Current;
					}
					else if (result is DataResponse)
					{
						var data = result as DataResponse;
						_context.Response.ContentType = data.ContentType;
						_context.Response.ContentEncoding = data.ContentEncoding;
						var output = _context.Response.OutputStream;
						yield return InvokeTaskAndWait(output.WriteAsync(data.Data, 0, data.Data.Length));
						if (!IsChildRequest)
						{
							output.Close();
							yield return Step.Current;
						}
					}
					else if (result is HttpCodeResponse)
					{
						HandlerHttpCodes(result as HttpCodeResponse, _context);
						yield break;
					}
				}
			}
			else
			{
				foreach (var filter in GlobalVars.GlobalFilters)
				{
					filter.OnPreExecute(_context);
				}

				//Resource handler should handle the result
				yield return HandleNotRoutedRequests(localPath, _context, new ExpandoObject());
			}

			if (!IsChildRequest)
			{
				StoreContext(_context);
			}
			ShouldTerminate = true;
			yield return Step.Current;
		}

		protected void InitializeCulture()
		{
			if (_listener == null)
			{
				return;
			}

			if (_listener != null && _listener.Context != null && 
				_listener.Context.Request != null && _listener.Context.Request.UserLanguages != null)
			{
				var userLanguages = _listener.Context.Request.UserLanguages;
				var langAvailable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var userLanguage in userLanguages)
				{
					var lang = userLanguage.Split(';')[0];
					langAvailable.Add(lang);
				}
				foreach (var lan in GlobalVars.Settings.Listener.Cultures.AvailableCultures)
				{
					var lk = lan.Key;
					if (langAvailable.Contains(lk))
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = lan.Value;
						_listenerCulture = lan.Value;
						return;
					}
					lk = lk.Substring(0, 2);
					if (langAvailable.Contains(lk))
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = lan.Value;
						_listenerCulture = lan.Value;
						return;
					}
				}
			}
			System.Threading.Thread.CurrentThread.CurrentCulture = GlobalVars.Settings.Listener.Cultures.DefaultCulture;
			_listenerCulture = GlobalVars.Settings.Listener.Cultures.DefaultCulture;
		}

		private static IEnumerable<Step> MakeControllerEnumerable(IEnumerable enumerable)
		{
			foreach (var item in enumerable)
			{
				var stepItem = item as Step;
				var stepResponse = item as StepResponse;
				if (stepResponse != null)
				{
					stepItem = stepResponse.CalleStep;
				}

				if (item == null) yield return Step.Current;
				else if (stepItem == null) yield return Step.DataStep(item);
				else yield return stepItem;
			}
		}

		/// <summary>
		/// Wait for completion and store the result into the container object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private Step InvokeControllerAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
		{
			if (typeof(T) != typeof(Step))
			{
				return InvokeLocalAndWait(() => MakeControllerEnumerable((IEnumerable)func()), result);
			}
			if (result == null) result = new Container();
			var enumerator = ((IEnumerable<Step>)func()).GetEnumerator();
			return Step.DataStep(new EnumeratorWrapper
								 {
									 Enum = enumerator,
									 Result = result,
									 Culture = System.Threading.Thread.CurrentThread.CurrentCulture
								 });
		}

		private static bool IsNotStaticRoute(RouteInstance routeDefintion)
		{
			return routeDefintion != null && !routeDefintion.StaticRoute &&
						 (routeDefintion.Parameters.ContainsKey("controller") && routeDefintion.Parameters.ContainsKey("action"));
		}



		public void StoreContext(NodeCsContext context, bool disconnect = true)
		{
			if (IsChildRequest) return;
			try
			{
				foreach (var filter in GlobalVars.GlobalFilters)
				{
					filter.OnPostExecute(context, null);
				}

				if (context.Session == null) throw new NullReferenceException("Missing session.");
				var session = (NodeCsHttpSession)context.Session;
				if (session.IsCleared || session.IsChanged)
				{
					var storedSid = context.Session.SessionID.Substring(1);
					GlobalVars.SessionStorage.StoreSession(storedSid, context.SessionData);
					context.UpdateSessionCookie();
				}
				else if (session.IsCleared)
				{
					var storedSid = context.Session.SessionID.Substring(1);
					GlobalVars.SessionStorage.ClearSession(storedSid);
					context.UpdateSessionCookie();
				}
			}
			catch (Exception ex)
			{
				Logger.Warn("Problem storing session\r\n{0}", ex);
			}
			try
			{
				if (disconnect) context.Response.Close();
			}
			catch (Exception ex)
			{
			}
		}

		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			GlobalVars.ExceptionManager.HandleException(ex, _context);
			StoreContext(_context);
		}

		public void Initialize(NodeCsServer nodeCsServer, HttpCoroutineListenerClient listener)
		{
			_listener = listener;
		}


		private void HandlerHttpCodes(HttpCodeResponse httpCodeResponse, NodeCsContext context)
		{
			StoreContext(_context, false);
			if (httpCodeResponse.HttpCode == 302)
			{
				var redirect = (RedirectResponse)httpCodeResponse;

				var urlHelper = new UrlHelper(context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				context.Response.Redirect(realUrl);
			}
			else if (httpCodeResponse.HttpCode == 301)
			{
				var redirect = (RedirectResponse)httpCodeResponse;

				var urlHelper = new UrlHelper(context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				context.Response.RedirectPermanent(realUrl);
			}
			context.Response.Close();
			ShouldTerminate = true;
		}

		internal Step HandleResult(IResponse result, NodeCsContext context, dynamic viewBag, ModelStateDictionary modelState)
		{
			var viewResult = result as ViewResponse;
			if (viewResult != null)
			{
				return HandleNotRoutedRequests(viewResult.View, context, viewBag, viewResult.Model, modelState);
			}
			return Step.Current;
		}

		internal bool IsSessionCapable(string localPath)
		{
			string foundedExt = null;
			if (!Path.HasExtension(localPath))
			{
				foreach (var ext in NodeCsServer.Instance.ExtensionHandler.Extensions)
				{
					var newPath = localPath + ext;
					var tmpReal = NodeCsServer.Instance.GetFilePath(newPath);
					if (tmpReal != null)
					{
						localPath += ext;
						foundedExt = ext;
						break;
					}
				}
			}
			if (foundedExt == null) return false;
			var foundedPath = NodeCsServer.Instance.GetFilePath(localPath);
			if (foundedPath == null) return false;
			return NodeCsServer.Instance.ExtensionHandler.IsSessionCapable(foundedExt);
		}

		internal Step HandleNotRoutedRequests(string localPath, NodeCsContext context,
			dynamic viewBag, object model = null, ModelStateDictionary modelState = null)
		{
			if (modelState == null) modelState = new ModelStateDictionary();
			if (!Path.HasExtension(localPath))
			{
				foreach (var ext in NodeCsServer.Instance.ExtensionHandler.Extensions)
				{
					var newPath = localPath + ext;
					var tmpReal = NodeCsServer.Instance.GetFilePath(newPath);
					if (tmpReal != null)
					{
						localPath += ext;
						break;
					}
				}
			}
			var foundedPath = NodeCsServer.Instance.GetFilePath(localPath);
			if (foundedPath == null)
			{
				throw new NodeCsException("Resource '{0}' not found.", 404, localPath);
			}

			var handlerInstance = NodeCsServer.Instance.ExtensionHandler.CreateInstance(context, foundedPath);
			_resourceHandler = handlerInstance as IResourceHandler;
			if (_resourceHandler != null)
			{
				_resourceHandler.Model = model;
				_resourceHandler.ViewBag = viewBag;
				_resourceHandler.ModelState = modelState;
				_resourceHandler.ViewData = ViewData;
			}

			return CallHandlerInstance(handlerInstance);
		}

		protected virtual Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			return InvokeCoroutineAndWait(NodeCsServer.Instance.NextCoroutine, handlerInstance);
		}

		public Dictionary<string, object> ViewData { get; set; }

		public OnHttpListenerReceivedCoroutine()
		{
			ViewData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}
	}
}
