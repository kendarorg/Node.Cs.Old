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
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using HttpMvc;
using HttpMvc.Controllers;
using NodeCs.Shared;
using CoroutinesLib.Shared;

namespace Http.Renderer.Razor
{
	public class RazorViewHandler:IResponseHandler
	{
		private readonly HttpModule _httpModule;
		private MvcModule _mvcModule;

		public RazorViewHandler()
		{
			_mvcModule = NodeRoot.GetModule("http.mvc") as MvcModule;
			_httpModule = ServiceLocator.Locator.Resolve<HttpModule>();
		}
		public IEnumerable<ICoroutineResult> Handle(IHttpContext context, IResponse response, object viewBag)
		{
			var viewResponse = (ViewResponse)response;
			var view = viewResponse.View ?? context.RouteParams["action"].ToString();
			
			if (view.StartsWith("~"))
			{
				view = view.TrimStart('~');
			}
			else
			{
				var viewsRoot = _mvcModule.GetParameter<string>("views").TrimStart('~').TrimStart('/');
				viewsRoot = viewsRoot.Replace("{controller}", context.RouteParams["controller"].ToString());
				viewsRoot = viewsRoot.Replace("{action}", view.Trim('/'));
				if (context.RouteParams.ContainsKey("area"))
				{
					viewsRoot = viewsRoot.Replace("{area}", context.RouteParams["area"].ToString());
				}
				view = "/" + viewsRoot.Trim('/');
				view = view.TrimStart('~');
			}

			if (!view.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
			{
				view += ".cshtml";
			}
			var wrappedContext = new WrappedHttpContext(context);
			var wrappedRequest = (IHttpRequest)wrappedContext.Request;
			wrappedRequest.SetInputStream(context.Request.InputStream);
			wrappedRequest.SetUrl(new Uri(view,UriKind.RelativeOrAbsolute));
			var wrappedResponse = (IHttpResponse)wrappedContext.Response;
			wrappedResponse.SetOutputStream(context.Response.OutputStream);
			yield return _httpModule.ExecuteRequestInternal(wrappedContext, viewResponse.Model, new ModelStateDictionary(), viewBag);
		}

		public bool CanHandle(IResponse response)
		{
			var viewResponse = response as ViewResponse;
			if (viewResponse == null) return false;
			if (string.IsNullOrWhiteSpace(viewResponse.View)) return false;
			return true;
		}
	}
}