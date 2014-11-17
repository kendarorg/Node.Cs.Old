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
using GenericHelpers;
using Http.Shared.Contexts;
using Http.Shared.Routing;

namespace HttpMvc.Controllers
{
	public class UrlHelper
	{
		private readonly IHttpContext _context;
		private readonly IRoutingHandler _routingHandler;

		public UrlHelper(IHttpContext context, IRoutingHandler routingHandler)
		{
			_context = context;
			_routingHandler = routingHandler;
		}

		public string RealRootAddress
		{
			get
			{
#if AAA
				var dataPath = NodeCsSettings.Settings.Listener.RootDir ?? string.Empty;
				dataPath = dataPath.Trim('/');
				if (dataPath.Length > 0)
				{
					dataPath = "/" + dataPath;
				}
#else
				if(new Random().Next()>=int.MinValue)
				throw new NotImplementedException();
#endif
				var dataPath = "";
				var url = _context.Request.Url;
				var port = url.Port.ToString();
				if (port != "80")
				{
					port = ":" + port;
				}
				else
				{
					port = "";
				}

				var realUrl = string.Format("{0}://{1}{2}/{3}",
					url.Scheme,
					url.Host,
					port,
					dataPath
					);
				return realUrl.TrimEnd('/');
			}
		}

		public string Content(string content)
		{
			//~/Scripts/jquery.validate.min.js
			if (content == "~") return "/";
			if (IsLocalUrl(content))
			{
				return RealRootAddress + "/" + content.Trim('~').TrimStart('/');
			}
			return content;
		}

		public string MergeUrl(string toMerge)
		{
			if (toMerge.StartsWith("~"))
			{
				toMerge = toMerge.Substring(1);
			}
			if (toMerge.StartsWith("/"))
			{
				toMerge = toMerge.TrimStart('/');
				return RealRootAddress + "/" + toMerge;
			}
			return toMerge;
		}

		public bool IsLocalUrl(string returnUrl)
		{
			if (returnUrl.StartsWith("~/") || returnUrl.StartsWith("/"))
			{
				return true;
			}
			return false;
		}

		public string Action(string action, dynamic routeValues)
		{
			var controller = _context.RouteParams["controller"].ToString();
			return Action(action, controller, routeValues);
		}

		public string Action(string action, string controller, dynamic routeValues)
		{
			var result = ReflectionUtils.ObjectToDictionary(routeValues);
			if (!result.ContainsKey("controller"))
			{
				result["controller"] = controller;
			}
			if (result.ContainsKey("action"))
			{
				result["action"] = action;
			}
			else
			{
				result.Add("action", action);
			}

			var link = _routingHandler.ResolveFromParams(result);
			return link;
		}

		public string Action(string action, string controller)
		{
			var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
			             {
				             {"controller", controller},
				             {"action", action}
			             };
			var link = _routingHandler.ResolveFromParams(result);
			return link;
		}
	}
}
