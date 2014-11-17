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
using System.Dynamic;
using System.Web;
using GenericHelpers;
using Http.Shared.Contexts;
using Http.Shared.Controllers;

namespace HttpMvc.Controllers
{
	public abstract class ControllerBase : MainControllerBase
	{
		protected IResponse NotFound(string url = null)
		{
			return new NotFoundResponse(url);
		}

		public HttpSessionStateBase Session
		{
			get
			{
				return HttpContext.Session;
			}
		}

		private dynamic _dynamicViewDataDictionary;

		private Dictionary<string, object> _viewData;


		protected IResponse Redirect(string url)
		{
			return new RedirectResponse(url);
		}

		public dynamic ViewBag
		{
			get
			{
				if (_dynamicViewDataDictionary == null)
				{
					_dynamicViewDataDictionary = new ExpandoObject();
				}
				return _dynamicViewDataDictionary;
			}
		}

		public Dictionary<string, object> ViewData
		{
			get
			{
				if (_viewData == null)
				{
					_viewData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				}
				return _viewData;
			}
		}

		public IResponse View(object model)
		{
			var action = HttpContext.RouteParams["action"].ToString();
			return View(action, model);
		}

		public IResponse View(string view, object model)
		{
			return new ViewResponse(view, model, ViewBag);
		}

		public IResponse View()
		{
			return View(new object());
		}

		protected IResponse PartialView(object model)
		{
			var action = (string)HttpContext.RouteParams["action"];
			if (model is string)
			{
				action = model.ToString();
				model = null;
			}
			return PartialView(action, model);
		}

		protected IResponse PartialView(string action, object model)
		{
			return new PartialViewResponse(action, model, ViewBag);
		}


		protected IResponse RedirectToAction(string action, string controller = null)
		{
			return RedirectToAction(action, null, controller);
		}

		protected IResponse RedirectToAction(string action, dynamic value, string controller = null)
		{
			Dictionary<string, object> pars;
			if (value != null)
			{
				pars = ReflectionUtils.ObjectToDictionary(value);
			}
			else
			{
				pars = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}

			controller = controller ?? (string)HttpContext.RouteParams["controller"];
			if (!pars.ContainsKey("controller"))
			{
				pars.Add("controller", controller);
			}
			if (!pars.ContainsKey("action"))
			{
				pars.Add("action", action);
			}
			//var redirectUrl = GlobalVars.RoutingService.ResolveFromParams(pars);
			//return Redirect(redirectUrl);
			throw new NotImplementedException();
		}
	}
}
