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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using ClassWrapper;
using ConcurrencyHelpers.Containers.Asyncs;
using ConcurrencyHelpers.Coroutines;
using NetworkHelpers.Http;
using Node.Cs.Lib.Attributes;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.OnReceive;
using Node.Cs.Lib.Routing;
using TB.ComponentModel;

namespace Node.Cs.Lib.Utils
{
	public class OnHttpListenerReceivedCoroutine : Coroutine
	{
		protected void RunGlobalPreFilters()
		{
			foreach (var filter in GlobalVars.GlobalFilters)
			{
				filter.OnPreExecute((HttpContextBase)_contextManager.Context);
			}
		}

		protected void RunGlobalPostFilters(IResponse result)
		{
			foreach (var filter in GlobalVars.GlobalFilters)
			{
				filter.OnPostExecute((HttpContextBase)_contextManager.Context, result);
			}
		}

		protected static void ReInitialize()
		{
			_pages = new AsyncLockFreeDictionary<string, PageDescriptor>(new Dictionary<string, PageDescriptor>());
		}

		private static AsyncLockFreeDictionary<string, PageDescriptor> _pages = new AsyncLockFreeDictionary<string, PageDescriptor>(new Dictionary<string, PageDescriptor>());
		private IListenerContainer _listener;
		//private INodeCsContext _context;
		private IResourceHandler _resourceHandler;
		private CultureInfo _listenerCulture;

		protected IEnumerable<Step> HandleControllerRoute()
		{
			RunGlobalPreFilters();
			var resultDate = new Container();
			yield return InvokeControllerAndWait(() =>
				HandleRoutedRequests(_contextManager.RouteDefintion, _contextManager.LocalPath, _contextManager.Context, IsChildRequest),
					resultDate);

			CheckException();

			var result = resultDate.RawData as IResponse;
			if (result != null)
			{
				RunGlobalPostFilters(result);
				var responseHandler = GlobalVars.ResponseHandlers.Load(result.GetType());
				yield return Step.Current;
				if (responseHandler != null)
				{
					//Its responsability to close connection
					var viewResult = result as IViewResponse;

					if (viewResult != null)
					{
						ViewData = viewResult.ViewData;
						yield return HandleNotRoutedRequests(viewResult.View, _contextManager.Context, viewResult.ViewBag, viewResult.Model, viewResult.ModelState);
					}
				}
				else
				{
					var dataResponse = result as DataResponse;
					if (dataResponse != null)
					{
						_contextManager.Context.Response.ContentType = dataResponse.ContentType;
						_contextManager.Context.Response.ContentEncoding = dataResponse.ContentEncoding;
						var output = _contextManager.Context.Response.OutputStream;
						yield return InvokeTaskAndWait(output.WriteAsync(dataResponse.Data, 0, dataResponse.Data.Length));
						if (!IsChildRequest)
						{
							output.Close();
						}
					}
					else
					{
						HandlerHttpCodes(result as HttpCodeResponse, _contextManager.Context);
					}
				}

			}
		}


		protected virtual bool IsChildRequest { get { return false; } }

		public override IEnumerable<Step> Run()
		{
			yield return _contextManager.InitializeContext();
			_contextManager.InitializeResponse();

			_sessionManager.InitializeSession(IsChildRequest, _contextManager);
			if (_sessionManager.SupportSession)
			{
				var actionResult = new Container();
				yield return InvokeLocalAndWait(() => GlobalVars.SessionStorage.CreateSession(_sessionManager.StoredSid), actionResult);
				_sessionManager.LoadSessionData(actionResult);
			}


			if (_contextManager.IsNotStaticRoute)
			{
				foreach (var step in HandleControllerRoute()) yield return step;
			}
			else
			{
				RunGlobalPreFilters();

				//Resource handler should handle the result
				yield return HandleNotRoutedRequests(_contextManager.LocalPath, _contextManager.Context, new ExpandoObject());

				RunGlobalPostFilters(null);
			}

			if (_sessionManager.SupportSession)
			{
				StoreContext(_contextManager.Context);
			}
			ShouldTerminate = true;
		}

		internal Step HandleNotRoutedRequests(string localPath, INodeCsContext context,
			dynamic viewBag, object model = null, ModelStateDictionary modelState = null)
		{
			if (modelState == null) modelState = new ModelStateDictionary();
			if (!Path.HasExtension(localPath))
			{
				foreach (var ext in GlobalVars.ExtensionHandler.Extensions)
				{
					var newPath = localPath + ext;
					var tmpReal = GetFilePath(newPath);
					if (tmpReal != null)
					{
						localPath += ext;
						break;
					}
				}
			}
			var foundedPath = GetFilePath(localPath);
			if (foundedPath == null)
			{
				throw new NodeCsException("Resource '{0}' not found.", 404, localPath);
			}

			var handlerInstance = GlobalVars.ExtensionHandler.CreateInstance((HttpContextBase)context, foundedPath);
			_resourceHandler = handlerInstance as IResourceHandler;
			if (_resourceHandler == null)
			{
				return Step.Current;
			}

			_resourceHandler.Model = model;
			_resourceHandler.ViewBag = viewBag;
			_resourceHandler.ModelState = modelState;
			_resourceHandler.ViewData = ViewData;


			return CallHandlerInstance(handlerInstance);
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
		protected virtual Step InvokeControllerAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
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




		public void StoreContext(INodeCsContext context, bool disconnect = true)
		{
			if (IsChildRequest) return;
			try
			{
				foreach (var filter in GlobalVars.GlobalFilters)
				{
					filter.OnPostExecute((HttpContextBase)context, null);
				}

				if (context.Session == null) throw new NullReferenceException("Missing session.");
				var session = (INodeCsSession)context.Session;
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
			GlobalVars.ExceptionManager.HandleException(ex, (HttpContextBase)_contextManager.Context);
			StoreContext(_contextManager.Context);
		}


		private ContextManager _contextManager;
		private SessionManager _sessionManager;
		public void Initialize(ContextManager contextManager, SessionManager sessionManager,

			INodeCsServer nodeCsServer, IListenerContainer listener)
		{
			_listener = listener;
			_contextManager = contextManager;
			_sessionManager = sessionManager;
		}


		private void HandlerHttpCodes(HttpCodeResponse httpCodeResponse, INodeCsContext context)
		{
			StoreContext(_contextManager.Context, false);
			if (httpCodeResponse.HttpCode == 302)
			{
				var redirect = (RedirectResponse)httpCodeResponse;

				var urlHelper = new UrlHelper((HttpContextBase)context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				context.Response.Redirect(realUrl);
			}
			else if (httpCodeResponse.HttpCode == 301)
			{
				var redirect = (RedirectResponse)httpCodeResponse;

				var urlHelper = new UrlHelper((HttpContextBase)context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				context.Response.RedirectPermanent(realUrl);
			}
			context.Response.Close();
			ShouldTerminate = true;
		}

		internal bool IsSessionCapable(string localPath)
		{
			string foundedExt = null;
			if (!Path.HasExtension(localPath))
			{
				foreach (var ext in GlobalVars.ExtensionHandler.Extensions)
				{
					var newPath = localPath + ext;
					var tmpReal = GetFilePath(newPath);
					if (tmpReal != null)
					{
						localPath += ext;
						foundedExt = ext;
						break;
					}
				}
			}
			if (foundedExt == null) return false;
			var foundedPath = GetFilePath(localPath);
			if (foundedPath == null) return false;
			return GlobalVars.ExtensionHandler.IsSessionCapable(foundedExt);
		}



		protected virtual Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			return InvokeCoroutineAndWait(GlobalVars.NodeCsServer.NextCoroutine, handlerInstance);
		}

		public Dictionary<string, object> ViewData { get; set; }

		public OnHttpListenerReceivedCoroutine()
		{
			ViewData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}


		private static PageDescriptor GetFilePath(string localPath)
		{
			localPath = PathCleanser.WebToFilePath(localPath);
			if (_pages.ContainsKey(localPath))
			{
				return _pages[localPath];
			}
			var de = GlobalVars.PathProvider.DirectoryExists(localPath);
			if (de)
			{
				var rpp = GlobalVars.PathProvider.GetProviderForFileNamed(localPath + "/index");
				var fn = rpp.GetFileNamed(localPath + "/index");
				var pd = new PageDescriptor
				{
					PathProvider = rpp,
					RealPath = fn
				};
				_pages.Add(localPath, pd);
				return pd;
			}
			var pp = GlobalVars.PathProvider.GetProviderForFile(localPath);
			if (pp != null)
			{
				var pd = new PageDescriptor
				{
					PathProvider = pp,
					RealPath = localPath
				};
				_pages.Add(localPath, pd);
				return pd;
			}
			return null;
		}


		public IEnumerable<Step> HandleRoutedRequests(RouteInstance routeInstance, string localPath,
			INodeCsContext context, bool isChildRequest = false)
		{
			var verb = context.Request.HttpMethod;
			object methResult = null;

			if (routeInstance.Parameters.ContainsKey("controller") &&
					routeInstance.Parameters.ContainsKey("action"))
			{

				var controllerName = routeInstance.Parameters["controller"].ToString();
				var allParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach (var param in routeInstance.Parameters)
				{
					allParams.Add(param.Key, param.Value);
				}
				foreach (var param in context.Request.QueryString.AllKeys)
				{
					if (param != null && !allParams.ContainsKey(param))
					{
						allParams.Add(param, context.Request.QueryString[param]);
					}
				}
				foreach (var param in context.Request.Form.AllKeys)
				{
					if (param != null && !allParams.ContainsKey(param))
					{
						allParams.Add(param, context.Request.Form[param]);
					}
				}

				var requestContentType = context.Request.ContentType;
				bool hasConverter = GlobalVars.ConversionService.HasConverter(requestContentType);

				ControllerWrapperInstance controller = null;

				controller = (ControllerWrapperInstance)GlobalVars.ControllersFactoryHandler.Create(controllerName + "Controller");
				if (controller == null)
				{
					yield return NullResponse.Instance;
					yield break;
				}
				controller.Instance.Set("HttpContext", (HttpContextBase)context);
				var action = routeInstance.Parameters["action"].ToString();
				var methods = controller.GetMethodGroup(action, verb).ToList();

				foreach (var attr in controller.Instance.Instance.GetType().GetCustomAttributes(typeof(FilterBase)))
				{
					var filter = attr as FilterBase;
					if (filter != null)
					{
						filter.OnPreExecute((HttpContextBase)context);
					}
				}

				bool methodInvoked = false;
				foreach (var method in methods)
				{
					if (TryInvoke(method, allParams, controller.Instance, hasConverter, (HttpContextBase)context, isChildRequest,
						out methResult))
					{
						methodInvoked = true;
						break;
					}
				}
				if (!methodInvoked)
				{
					throw new NodeCsException("Missing action '{0}' for verb '{1}'", 404, action, verb);
				}

				var enumerableResult = methResult as IEnumerable<IResponse>;

				var result = new Container();
				yield return Coroutine.InvokeLocalAndWait(() => EnumerateResponse(enumerableResult), result);

				var typeofResponse = result.RawData.GetType();
				var responseHandler = GlobalVars.ResponseHandlers.Load(typeofResponse);
				var response = result.RawData as IResponse;
				if (responseHandler != null && response != null)
				{
					responseHandler.Handle(controller, context, response);
					yield return Step.DataStep(response);
				}
				else
				{
					yield return Step.DataStep(result.RawData);
				}

				if (controller != null)
				{
					GlobalVars.ControllersFactoryHandler.Release((IController)controller.Instance.Instance);
				}
				yield break;
			}
		}


		private IEnumerable<Step> EnumerateResponse(IEnumerable<IResponse> enumerableResult)
		{
			foreach (var item in enumerableResult)
			{
				var calleeStep = item as StepResponse;
				var itemStep = item as Step;
				if (calleeStep != null)
				{
					yield return calleeStep.CalleStep;
				}
				else if (itemStep != null)
				{
					yield return itemStep;
				}
				else
				{
					yield return Step.DataStep(item);
					yield break;
				}
			}
		}

		private bool TryInvoke(MethodWrapperDescriptor method,
			Dictionary<string, object> allParams,
			global::ClassWrapper.ClassWrapper controllerWrapper, bool hasConverter,
			HttpContextBase context, bool isChildRequest, out object methResult)
		{
			var request = context.Request;
			var parsValues = new List<object>();
			methResult = null;
			var methPars = method.Parameters.ToList();

			for (int index = 0; index < methPars.Count; index++)
			{
				bool parValueSet = false;
				var par = methPars[index];
				object valueToAdd = null;

				if (allParams.ContainsKey(par.Name))
				{
					var parValue = allParams[par.Name];
					if (parValue.GetType() != par.ParameterType)
					{
						object convertedValue;
						if (UniversalTypeConverter.TryConvert(parValue, par.ParameterType, out convertedValue))
						{
							valueToAdd = convertedValue;
							parValueSet = true;
						}
						else if (!par.HasDefault)
						{
							if (par.ParameterType.IsValueType)
							{
								return false;
							}
							parValueSet = true;
						}
					}
					else
					{
						valueToAdd = parValue;
						parValueSet = true;
					}

				}
				if (par.ParameterType == typeof(FormCollection))
				{
					parValueSet = true;
					valueToAdd = new FormCollection(context.Request.Form);
				}
				if (parValueSet == false && request.ContentType != null)
				{
					var parType = par.ParameterType;
					if (!parType.IsValueType &&
							!parType.IsArray &&
							!(parType == typeof(string)) &&
							!parType.IsEnum &&
							!(parType == typeof(object)))
					{
						try
						{
							valueToAdd = GlobalVars.ConversionService.Convert(parType, request.ContentType, request);
							parValueSet = true;
						}
						catch (Exception)
						{

						}
					}
				}
				if (par.HasDefault && !parValueSet)
				{
					parValueSet = true;
					valueToAdd = par.Default;
				}

				if (!parValueSet && string.Compare(par.Name, "returnUrl", StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (request.UrlReferrer != null)
					{
						parValueSet = true;
						valueToAdd = request.UrlReferrer.ToString();
					}
				}

				if (!par.GetType().IsValueType && !parValueSet)
				{
					parValueSet = true;
					valueToAdd = null;
				}

				if (!parValueSet) return false;
				parsValues.Add(valueToAdd);
			}

			var attributes = new List<Attribute>(method.Attributes);
			foreach (var attribute in attributes)
			{
				var filter = attribute as IFilter;

				if (filter != null)
				{
					if (!filter.OnPreExecute(context))
					{
						methResult = NullResponse.Instance;
						return true;
					}
				}
				else if (attribute is ChildActionOnly && !isChildRequest)
				{
					throw new NodeCsException("Item not found '{0}'.",404,_contextManager.Context.Request.Url.ToString());
				}
			}
			var msd = new ModelStateDictionary();
			foreach (var par in parsValues)
			{
				if (ValidationAttributesService.CanValidate(par))
				{
					ValidationAttributesService.ValidateModel(par, msd);
				}

			}
			controllerWrapper.Set("ModelState", msd);
			var result = controllerWrapper.TryInvoke(method, out methResult, parsValues.ToArray());
			if (result)
			{
				foreach (var attribute in attributes)
				{
					var filter = attribute as IFilter;
					if (filter != null)
					{
						filter.OnPostExecute(context, null);
					}
				}
			}
			return result;
		}
	}


}
