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


using ClassWrapper;
using CoroutinesLib.Shared;
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.Routing;
using HttpMvc.Controllers;
using NodeCs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using TB.ComponentModel;

namespace Http.Coroutines
{
	public class RoutedItemCoroutine : CoroutineBase
	{
		private readonly RouteInstance _match;
		private readonly IControllerWrapperInstance _controller;
		private readonly IHttpContext _context;
		private readonly IConversionService _conversionService;

		public RoutedItemCoroutine(RouteInstance match, IControllerWrapperInstance controllerInstance, IHttpContext context, IConversionService conversionService, object model)
		{
			InstanceName = "RouteItem(" + string.Join(",", match.Parameters.Keys) + ")";
			_match = match;
			_context = context;
			_controller = controllerInstance;
			_conversionService = conversionService;
		}

		public override void Initialize()
		{
			var verb = _context.Request.HttpMethod;
			object methResult = null;
			var allParams = ReadRequestParameters();
			var action = allParams.ContainsKey("action") ? allParams["action"].ToString() : null;
			var requestContentType = _context.Request.ContentType;
			bool hasConverter = _conversionService.HasConverter(requestContentType);

			var routeService = ServiceLocator.Locator.Resolve<IRoutingHandler>();
			var cdd = _controller.WrapperDescriptor;
			for (int index = 0; index < _controller.Instance.Properties.Count; index++)
			{
				var property = _controller.Instance.Properties[index];
				switch (property)
				{
					case ("HttpContext"):
						_controller.Instance.Set(property, _context);
						break;
					case ("Url"):
						_controller.Instance.Set(property, new UrlHelper(_context, routeService));
						break;
					default:
						var prop = cdd.GetProperty(property);
						if (prop.SetterVisibility == ItemVisibility.Public)
						{
							var value = ServiceLocator.Locator.Resolve(prop.PropertyType);
							if (value != null)
							{
								_controller.Instance.Set(property, value);
							}
						}
						break;
				}
			}

			var methods = _controller.GetMethodGroup(action, verb).ToList();


			bool methodInvoked = false;
			foreach (var method in methods)
			{
				if (TryInvoke(method, allParams, _controller.Instance, hasConverter, _context, false,
						out methResult))
				{
					methodInvoked = true;
					break;
				}
			}
			if (!methodInvoked)
			{
				throw new HttpException(404, string.Format("Url '{0}' not found.", _context.Request.Url));
			}

			_enumerableResult = (methResult as IEnumerable<IResponse>).GetEnumerator();
		}

		private IEnumerator<IResponse> _enumerableResult;
		private bool _iInitializedSession = false;

		public override IEnumerable<ICoroutineResult> OnCycle()
		{
			if (_context.Session == null)
			{
				var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
				if (sessionManager != null)
				{
					_iInitializedSession = true;
					sessionManager.InitializeSession(_context);
				}
			}
			if (_enumerableResult.MoveNext())
			{
				var result = _enumerableResult.Current as WaitResponse;
				if (result != null)
				{
					yield return result.Result;
				}
				else if (!(_enumerableResult.Current is NullResponse))
				{
					IResponse response = _enumerableResult.Current;
					ServiceLocator.Locator.Release(_controller.Instance.Instance);

					var httpModule = ServiceLocator.Locator.Resolve<HttpModule>();
					foreach (var item in httpModule.HandleResponse(_context, response))
					{
						yield return item;
					}
				}
			}
			else
			{
				if (_iInitializedSession)
				{
					if (_context.Session != null)
					{
						var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
						sessionManager.SaveSession(_context);
					}
				}
				if (_context.Parent == null)
				{
					_context.Response.Close();
				}
				TerminateElaboration();
			}

		}

		private Dictionary<string, object> ReadRequestParameters()
		{
			var allParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			foreach (var param in _match.Parameters)
			{
				allParams.Add(param.Key, param.Value);
			}
			foreach (var param in _context.Request.QueryString.AllKeys)
			{
				if (param != null && !allParams.ContainsKey(param))
				{
					allParams.Add(param, _context.Request.QueryString[param]);
				}
			}
			foreach (var param in _context.Request.Form.AllKeys)
			{
				if (param != null && !allParams.ContainsKey(param))
				{
					allParams.Add(param, _context.Request.Form[param]);
				}
			}
			return allParams;
		}

		public override void OnEndOfCycle()
		{

		}

		public override bool OnError(Exception exception)
		{
			if (_controller != null)
			{
				ServiceLocator.Locator.Release(_controller.Instance.Instance);
			}
			if (_iInitializedSession)
			{
				if (_context.Session != null)
				{
					var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
					sessionManager.SaveSession(_context);
				}
			}
			if (_context.Parent == null)
			{
				_context.Response.Close();
			}

			throw exception;
		}

		private bool TryInvoke(MethodWrapperDescriptor method,
				Dictionary<string, object> allParams,
				ClassWrapper.ClassWrapper controllerWrapper, bool hasConverter,
				IHttpContext context, bool isChildRequest, out object methResult)
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
								valueToAdd = _conversionService.Convert(parType, request.ContentType, request);
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
						throw new HttpException(404, string.Format("Url '{0}' not found.", _context.Request.Url));
					}
				}
				var msd = new ModelStateDictionary();
				foreach (var par in parsValues)
				{
					if (ValidationService.CanValidate(par))
					{
						var validationResult = ValidationService.ValidateModel(par);
						foreach (var singleResult in validationResult)
						{
							msd.AddModelError(singleResult.Property, singleResult.Message);
						}
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
							filter.OnPostExecute(context);
						}
					}
				}
				return result;
			}
			catch (Exception)
			{
				Log.Info("Not found suitable action for method '{0}'.", method.Name);
				methResult = null;
				return false;
			}
		}
	}
}
