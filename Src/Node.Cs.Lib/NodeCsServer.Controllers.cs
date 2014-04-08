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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using ClassWrapper;
using NetworkHelpers.Http;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Utils;
using TB.ComponentModel;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Attributes;

namespace Node.Cs.Lib
{
	public partial class NodeCsServer
	{
		private ControllersFactoryHandler _controllerFactoryHandler;

		private void InitializeControllersFactory()
		{
			_controllerFactoryHandler = new ControllersFactoryHandler();

			if (GlobalVars.ControllersFactory == null)
			{
				_controllerFactoryHandler.Initialize(GlobalVars.Settings.Factories.ControllersFactory);
			}
		}


		public IEnumerable<Step> HandleRoutedRequests(RouteInstance routeInstance, string localPath,
			NodeCsContext context, bool isChildRequest = false)
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

				controller = ControllersFactoryHandler.Create(controllerName + "Controller");
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
						filter.OnPreExecute(context);
					}
				}

				bool methodInvoked = false;
				foreach (var method in methods)
				{
					if (TryInvoke(method, allParams, controller.Instance, hasConverter, context,isChildRequest,
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

				var resultView = result.RawData as ViewResponse;
				if (resultView != null)
				{
					resultView.ModelState = controller.Instance.Get<ModelStateDictionary>("ModelState");
					resultView.ViewData = controller.Instance.Get<Dictionary<string, object>>("ViewData");
					yield return Step.DataStep(resultView);
				}
				else
				{
					yield return Step.DataStep(result.RawData);
				}

				if (controller != null)
				{
					ControllersFactoryHandler.Release((IController)controller.Instance.Instance);
				}
				yield break;
			}
		}

		private IEnumerable<Step> EnumerateResponse(IEnumerable<IResponse> enumerableResult)
		{
			foreach (var item in enumerableResult)
			{
				var calleeStep = item as StepResponse;
				if (calleeStep != null)
				{
					yield return calleeStep.CalleStep;
				}
				else if (item is Step)
				{
					yield return item as Step;
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
			var isChildActionOnly = false;
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
				else if (attribute is ChildActionOnly && isChildActionOnly)
				{
					methResult = new NotFoundResponse(context.Request.Url.ToString());
					return true;
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
