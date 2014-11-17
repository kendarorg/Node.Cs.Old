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
using System.Linq;
using System.Text;
using System.Web;
using ClassWrapper;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Attributes;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Utils;
using TB.ComponentModel;
using System.IO;
using Node.Cs.Lib.Loggers;

namespace Node.Cs.Lib.OnReceive
{
	public class ControllersManagerCoroutine : Coroutine, IControllersManagerCoroutine
	{
		public ControllersManagerCoroutine()
		{
			RouteInstanceParams = new Dictionary<string, object>();
		}

		public bool IsChildRequest { get; set; }
		//public ContextManager ContextManager { get; set; }
		public INodeCsContext Context { get; set; }
		public RouteInstance RouteDefintion { get; set; } 
		public string LocalPath { get; set; }
		public Dictionary<string, object> ViewData { get; set; }
		public ISessionManager SessionManager { get; set; }
		public string ActionName { get; set; }
		public string ControllerName { get; set; }
		public Dictionary<string,object>  RouteInstanceParams { get; set; }

		protected void RunGlobalPostFilters(IResponse result)
		{
			if (IsChildRequest) return;
			foreach (var filter in GlobalVars.GlobalFilters)
			{
				filter.OnPostExecute((HttpContextBase)Context, result);
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
			try
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
						throw new NodeCsException("Item not found '{0}'.", 404, Context.Request.Url.ToString());
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
			catch (Exception)
			{
				Logger.Info("Not found suitable action for method '{0}'.", method.Name);
				methResult = null;
				return false;
			}
		}

		public IEnumerable<Step> HandleRoutedRequests(RouteInstance routeInstance, string localPath,
			INodeCsContext context, bool isChildRequest = false)
		{
			var verb = context.Request.HttpMethod;
			object methResult = null;

			if (!string.IsNullOrEmpty(ControllerName) &&
					!string.IsNullOrEmpty(ActionName))
			{

				
				var allParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach (var param in RouteInstanceParams)
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

				var controller = (ControllerWrapperInstance)GlobalVars.ControllersFactoryHandler.Create(ControllerName + "Controller");
				if (controller == null)
				{
					throw new NodeCsException("Url '{0}' not found.",404,Context.Request.Url.ToString());
				}
				controller.Instance.Set("HttpContext", (HttpContextBase)context);
				
				var methods = controller.GetMethodGroup(ActionName, verb).ToList();

				
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
					throw new NodeCsException("Url '{0}' not found.", 404, Context.Request.Url.ToString());
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
			throw new NodeCsException("Url '{0}' not found.", 404, Context.Request.Url.ToString());
		}


		public override IEnumerable<Step> Run()
		{

			var resultDate = new Container();
			yield return InvokeControllerAndWait(() =>
				HandleRoutedRequests(RouteDefintion, LocalPath, Context, IsChildRequest),
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

						if (!IsChildRequest)
						{
							Thread.AddCoroutine(BuildViewManager(viewResult));
						}
						else
						{
							yield return InvokeCoroutineAndWait(BuildViewManager(viewResult));
						}
					}
				}
				else
				{
					var dataResponse = result as DataResponse;
					if (dataResponse != null)
					{
						Context.Response.ContentType = dataResponse.ContentType;
						Context.Response.ContentEncoding = dataResponse.ContentEncoding;
						var output = Context.Response.OutputStream;;
						((IForcedHeadersResponse)Context.Response).SetContentLength(dataResponse.Data.Length);
#if !TESTHTTP
						yield return InvokeTaskAndWait(output.WriteAsync(dataResponse.Data, 0, dataResponse.Data.Length));
						if (!IsChildRequest)
						{
							output.Close();
						}
#else
						if (IsChildRequest)
						{
							yield return InvokeTaskAndWait(output.WriteAsync(dataResponse.Data, 0, dataResponse.Data.Length));
						}
						else
						{
							HttpSender.Send(output, dataResponse.Data, true);
						}
#endif
					}
					else
					{
						var codesManager = new HttpCodesManager();
						codesManager.Response = result as HttpCodeResponse;
						codesManager.Context = Context;
						codesManager.SessionManager = SessionManager;
						Thread.AddCoroutine(codesManager);
					}
				}

			}
			ShouldTerminate = true;
		}

		public IPagesManager PagesManager { get; set; }

		protected virtual ViewsManagerCoroutine BuildViewManager(IViewResponse viewResult)
		{
			var viewsManager = new ViewsManagerCoroutine();
			viewsManager.PagesManager = PagesManager;
			viewsManager.LocalPath = viewResult.View;
			viewsManager.ViewData = ViewData;
			viewsManager.IsChildRequest = IsChildRequest;
			viewsManager.Context = Context;
			viewsManager.SessionManager = SessionManager;
			viewsManager.ViewBag = viewResult.ViewBag;
			viewsManager.Model = viewResult.Model;
			viewsManager.ModelState = viewResult.ModelState;
			if (IsChildRequest)
			{
				viewsManager.StringBuilder = StringBuilder;
			}

			return viewsManager;
		}

		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			GlobalVars.ExceptionManager.HandleException(ex, (HttpContextBase)Context);
			Context.Response.Close();
		}

		public void InitializeRouteInstance()
		{
			RouteInstanceParams = RouteDefintion.Parameters;
			if (RouteDefintion.Parameters.ContainsKey("controller"))
			{
				ControllerName = RouteDefintion.Parameters["controller"].ToString();
			}
			if (RouteDefintion.Parameters.ContainsKey("action"))
			{
				ActionName = RouteDefintion.Parameters["action"].ToString();
			}
		}

		public StringBuilder StringBuilder { get; set; }
	}
}
