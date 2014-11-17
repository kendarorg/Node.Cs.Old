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


using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System;
using System.Web.WebSockets;
using System.Threading.Tasks;
using System.Globalization;
using System.Web.SessionState;
using System.Web.Configuration;
using System.Web.Caching;
using System.Collections;
using System.Web.Instrumentation;
using System.Web.Profile;
using System.Security.Principal;
using System.Collections.Generic;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class FromBaseHttpContext : HttpContextBase, IHttpContext
	{
		public FromBaseHttpContext()
		{
			RouteParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public Dictionary<string, object> RouteParams { get; set; }
		public object SourceObject { get { return _httpContextBase; } }

		private static readonly MethodInfo _getInnerCollection;
		static FromBaseHttpContext()
		{
			var innerCollectionProperty = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.NonPublic | BindingFlags.Instance);
			_getInnerCollection = innerCollectionProperty.GetGetMethod(true);
		}

		public void ForceHeader(string key, string value)
		{
			var nameValueCollection = (NameValueCollection)_getInnerCollection.Invoke(_httpContextBase.Request.Headers, new object[] { });
			if (!_httpContextBase.Request.Headers.AllKeys.ToArray().Contains(key))
			{
				nameValueCollection.Add(key, value);
			}
			else
			{
				nameValueCollection.Set(key, value);
			}
		}

		private readonly HttpContextBase _httpContextBase;
		private FromBaseHttpResponse _response;
		private FromBaseHttpRequest _request;

		public FromBaseHttpContext(HttpContextBase httpContextBase)
		{
			if (!(httpContextBase is SimpleHttpContext)) ContextsManager.OnOpen();
			_httpContextBase = httpContextBase;

			InitializeUnsettable();
		}


		public IHttpContext Parent
		{
			get { return null; }
		}

		public override ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback)
		{
			return _httpContextBase.AddOnRequestCompleted(callback);
		}

		public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc)
		{
			_httpContextBase.AcceptWebSocketRequest(userFunc);
		}

		public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options)
		{
			_httpContextBase.AcceptWebSocketRequest(userFunc, options);
		}

		public override void AddError(Exception errorInfo)
		{
			_httpContextBase.AddError(errorInfo);
		}

		public override void ClearError()
		{
			_httpContextBase.ClearError();
		}

		public override ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target)
		{
			return _httpContextBase.DisposeOnPipelineCompleted(target);
		}

		public override Object GetGlobalResourceObject(String classKey, String resourceKey)
		{
			return _httpContextBase.GetGlobalResourceObject(classKey, resourceKey);
		}

		public override Object GetGlobalResourceObject(String classKey, String resourceKey, CultureInfo culture)
		{
			return _httpContextBase.GetGlobalResourceObject(classKey, resourceKey, culture);
		}

		public override Object GetLocalResourceObject(String virtualPath, String resourceKey)
		{
			return _httpContextBase.GetLocalResourceObject(virtualPath, resourceKey);
		}

		public override Object GetLocalResourceObject(String virtualPath, String resourceKey, CultureInfo culture)
		{
			return _httpContextBase.GetLocalResourceObject(virtualPath, resourceKey, culture);
		}

		public override Object GetSection(String sectionName)
		{
			return _httpContextBase.GetSection(sectionName);
		}

		public override void RemapHandler(IHttpHandler handler)
		{
			_httpContextBase.RemapHandler(handler);
		}

		public override void RewritePath(String path)
		{
			_httpContextBase.RewritePath(path);
		}

		public override void RewritePath(String path, Boolean rebaseClientPath)
		{
			_httpContextBase.RewritePath(path, rebaseClientPath);
		}

		public override void RewritePath(String filePath, String pathInfo, String queryString)
		{
			_httpContextBase.RewritePath(filePath, pathInfo, queryString);
		}

		public override void RewritePath(String filePath, String pathInfo, String queryString, Boolean setClientFilePath)
		{
			_httpContextBase.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
		}

		public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
		{
			_httpContextBase.SetSessionStateBehavior(sessionStateBehavior);
		}

		public override Object GetService(Type serviceType)
		{
			return _httpContextBase.GetService(serviceType);
		}

		public override Exception[] AllErrors { get { return _httpContextBase.AllErrors; } }

		public void SetAllErrors(Exception[] val)
		{
		}

		public override Boolean AllowAsyncDuringSyncStages
		{
			set
			{
				_httpContextBase.AllowAsyncDuringSyncStages = value;
			}
			get
			{
				return _httpContextBase.AllowAsyncDuringSyncStages;
			}
		}


		public override HttpApplicationStateBase Application { get { return _httpContextBase.Application; } }

		public void SetApplication(HttpApplicationStateBase val)
		{
		}

		public override HttpApplication ApplicationInstance
		{
			set
			{
				_httpContextBase.ApplicationInstance = value;
			}
			get
			{
				return _httpContextBase.ApplicationInstance;
			}
		}

		public override AsyncPreloadModeFlags AsyncPreloadMode
		{
			set
			{
				_httpContextBase.AsyncPreloadMode = value;
			}
			get
			{
				return _httpContextBase.AsyncPreloadMode;
			}
		}


		public override Cache Cache { get { return _httpContextBase.Cache; } }

		public void SetCache(Cache val)
		{
		}


		public override IHttpHandler CurrentHandler { get { return _httpContextBase.CurrentHandler; } }

		public void SetCurrentHandler(IHttpHandler val)
		{
		}


		public override RequestNotification CurrentNotification { get { return _httpContextBase.CurrentNotification; } }

		public void SetCurrentNotification(RequestNotification val)
		{
		}


		public override Exception Error { get { return _httpContextBase.Error; } }

		public void SetError(Exception val)
		{
		}

		public override IHttpHandler Handler
		{
			set
			{
				_httpContextBase.Handler = value;
			}
			get
			{
				return _httpContextBase.Handler;
			}
		}


		public override Boolean IsCustomErrorEnabled { get { return _httpContextBase.IsCustomErrorEnabled; } }

		public void SetIsCustomErrorEnabled(Boolean val)
		{
		}


		public override Boolean IsDebuggingEnabled { get { return _httpContextBase.IsDebuggingEnabled; } }

		public void SetIsDebuggingEnabled(Boolean val)
		{
		}


		public override Boolean IsPostNotification { get { return _httpContextBase.IsPostNotification; } }

		public void SetIsPostNotification(Boolean val)
		{
		}


		public override Boolean IsWebSocketRequest { get { return _httpContextBase.IsWebSocketRequest; } }

		public void SetIsWebSocketRequest(Boolean val)
		{
		}


		public override Boolean IsWebSocketRequestUpgrading { get { return _httpContextBase.IsWebSocketRequestUpgrading; } }

		public void SetIsWebSocketRequestUpgrading(Boolean val)
		{
		}


		public override IDictionary Items { get { return _httpContextBase.Items; } }

		public void SetItems(IDictionary val)
		{
		}


		public override PageInstrumentationService PageInstrumentation { get { return _httpContextBase.PageInstrumentation; } }

		public void SetPageInstrumentation(PageInstrumentationService val)
		{
		}


		public override IHttpHandler PreviousHandler { get { return _httpContextBase.PreviousHandler; } }

		public void SetPreviousHandler(IHttpHandler val)
		{
		}


		public override ProfileBase Profile { get { return _httpContextBase.Profile; } }

		public void SetProfile(ProfileBase val)
		{
		}


		public override HttpRequestBase Request { get { return _httpContextBase.Request; } }

		public void SetRequest(HttpRequestBase val)
		{
		}


		public override HttpResponseBase Response { get { return _httpContextBase.Response; } }

		public void SetResponse(HttpResponseBase val)
		{
		}


		public override HttpServerUtilityBase Server { get { return _httpContextBase.Server; } }

		public void SetServer(HttpServerUtilityBase val)
		{
		}


		public override HttpSessionStateBase Session { get { return _httpContextBase.Session; } }

		public void SetSession(HttpSessionStateBase val)
		{
		}

		public override Boolean SkipAuthorization
		{
			set
			{
				_httpContextBase.SkipAuthorization = value;
			}
			get
			{
				return _httpContextBase.SkipAuthorization;
			}
		}


		public override DateTime Timestamp { get { return _httpContextBase.Timestamp; } }

		public void SetTimestamp(DateTime val)
		{
		}

		public override Boolean ThreadAbortOnTimeout
		{
			set
			{
				_httpContextBase.ThreadAbortOnTimeout = value;
			}
			get
			{
				return _httpContextBase.ThreadAbortOnTimeout;
			}
		}


		public override TraceContext Trace { get { return _httpContextBase.Trace; } }

		public void SetTrace(TraceContext val)
		{
		}

		public override IPrincipal User
		{
			set
			{
				_httpContextBase.User = value;
			}
			get
			{
				return _httpContextBase.User;
			}
		}


		public override String WebSocketNegotiatedProtocol { get { return _httpContextBase.WebSocketNegotiatedProtocol; } }

		public void SetWebSocketNegotiatedProtocol(String val)
		{
		}


		public override IList<String> WebSocketRequestedProtocols { get { return _httpContextBase.WebSocketRequestedProtocols; } }

		public void SetWebSocketRequestedProtocols(IList<String> val)
		{
		}
		public void InitializeUnsettable()
		{
			_response = new FromBaseHttpResponse(_httpContextBase.Response);
			_request = new FromBaseHttpRequest(_httpContextBase.Request);
			_response.ContentEncoding = _request.ContentEncoding;
		}


		public Task InitializeWebSocket()
		{
			throw new NotImplementedException();
		}
	}
}