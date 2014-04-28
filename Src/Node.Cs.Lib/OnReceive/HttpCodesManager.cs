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
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;

namespace Node.Cs.Lib.OnReceive
{
	public class HttpCodesManager:Coroutine, IHttpCodesManager
	{
		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			GlobalVars.ExceptionManager.HandleException(ex, (HttpContextBase)Context);
			Context.Response.Close();
		}

		public override IEnumerable<Step> Run()
		{
			SessionManager.StoreContext(Context, false);
			if (Response.HttpCode == 302)
			{
				var redirect = (RedirectResponse)Response;

				var urlHelper = new UrlHelper((HttpContextBase)Context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				Context.Response.Redirect(realUrl);
			}
			else if (Response.HttpCode == 301)
			{
				var redirect = (RedirectResponse)Response;

				var urlHelper = new UrlHelper((HttpContextBase)Context);
				var realUrl = urlHelper.MergeUrl(redirect.Url);
				Context.Response.RedirectPermanent(realUrl);
			}
			Context.Response.Close();
			ShouldTerminate = true;
			yield break;
		}

		public HttpCodeResponse Response { get; set; }

		public INodeCsContext Context { get; set; }

		public ISessionManager SessionManager { get; set; }
	}
}
