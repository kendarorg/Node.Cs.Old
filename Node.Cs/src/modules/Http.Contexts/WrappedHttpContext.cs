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


using Http.Shared.Contexts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Instrumentation;
using System.Web.Profile;
using System.Web.SessionState;
using System.Web.WebSockets;

namespace Http.Contexts
{
	public class WrappedHttpContext : HttpContextBase, IHttpContext
	{
		public WrappedHttpContext()
		{
			RouteParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public IHttpContext RootContext
		{
			get
			{
				if (Parent == null) return this;
				return Parent.RootContext;
			}
		}

		public Dictionary<string, object> RouteParams { get; set; }
		private readonly IHttpContext _httpContext;

		public object SourceObject { get { return _httpContext; } }

		private static readonly MethodInfo _getInnerCollection;
		static WrappedHttpContext()
		{
			var innerCollectionProperty = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.NonPublic | BindingFlags.Instance);
			_getInnerCollection = innerCollectionProperty.GetGetMethod(true);
		}

		public void ForceHeader(string key, string value)
		{
			var nameValueCollection = (NameValueCollection)_getInnerCollection.Invoke(_response.Headers, new object[] { });
			if (!_response.Headers.AllKeys.ToArray().Contains(key))
			{
				nameValueCollection.Add(key, value);
			}
			else
			{
				nameValueCollection.Set(key, value);
			}
		}

		public WrappedHttpContext(IHttpContext httpContext)
		{
			_httpContext = httpContext;
			
			InitializeUnsettable();
		}

		public IHttpContext Parent
		{
			get { return _httpContext; }
		}
		public override ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback)
		{
			throw new NotImplementedException();
		}

		public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc)
		{
			_httpContext.AcceptWebSocketRequest(userFunc);
		}

		public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options)
		{
			_httpContext.AcceptWebSocketRequest(userFunc, options);
		}

		public override void AddError(Exception errorInfo)
		{
			_httpContext.AddError(errorInfo);
		}

		public override void ClearError()
		{
			_httpContext.ClearError();
		}

		public override ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target)
		{
			return _httpContext.DisposeOnPipelineCompleted(target);
		}

		public override Object GetGlobalResourceObject(String classKey, String resourceKey)
		{
			//TODO Missing GetGlobalResourceObject for HttpContext
			return null;
		}

		public override Object GetGlobalResourceObject(String classKey, String resourceKey, CultureInfo culture)
		{
			//TODO Missing GetGlobalResourceObject for HttpContext
			return null;
		}

		public override Object GetLocalResourceObject(String virtualPath, String resourceKey)
		{
			//TODO Missing GetLocalResourceObject for HttpContext
			return null;
		}

		public override Object GetLocalResourceObject(String virtualPath, String resourceKey, CultureInfo culture)
		{
			//TODO Missing GetLocalResourceObject for HttpContext
			return null;
		}

		public override Object GetSection(String sectionName)
		{
			return _httpContext.GetSection(sectionName);
		}

		public override void RemapHandler(IHttpHandler handler)
		{
			_httpContext.RemapHandler(handler);
		}

		public override void RewritePath(String path)
		{
			_httpContext.RewritePath(path);
		}

		public override void RewritePath(String path, Boolean rebaseClientPath)
		{
			_httpContext.RewritePath(path, rebaseClientPath);
		}

		public override void RewritePath(String filePath, String pathInfo, String queryString)
		{
			_httpContext.RewritePath(filePath, pathInfo, queryString);
		}

		public override void RewritePath(String filePath, String pathInfo, String queryString, Boolean setClientFilePath)
		{
			_httpContext.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
		}

		public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
		{
			_httpContext.SetSessionStateBehavior(sessionStateBehavior);
		}

		public override Object GetService(Type serviceType)
		{
			//TODO Missing GetService for HttpContext
			return null;
		}

		public override Exception[] AllErrors { get { return _httpContext.AllErrors; } }

		public void SetAllErrors(Exception[] val)
		{
		}

		public override Boolean AllowAsyncDuringSyncStages
		{
			set
			{
				_httpContext.AllowAsyncDuringSyncStages = value;
			}
			get
			{
				return _httpContext.AllowAsyncDuringSyncStages;
			}
		}


		public override HttpApplication ApplicationInstance
		{
			set
			{
				_httpContext.ApplicationInstance = value;
			}
			get
			{
				return _httpContext.ApplicationInstance;
			}
		}

		public override AsyncPreloadModeFlags AsyncPreloadMode
		{
			set
			{
				_httpContext.AsyncPreloadMode = value;
			}
			get
			{
				return _httpContext.AsyncPreloadMode;
			}
		}


		public override Cache Cache { get { return _httpContext.Cache; } }

		public void SetCache(Cache val)
		{
		}


		public override IHttpHandler CurrentHandler { get { return _httpContext.CurrentHandler; } }

		public void SetCurrentHandler(IHttpHandler val)
		{
		}


		public override RequestNotification CurrentNotification { get { return _httpContext.CurrentNotification; } }

		public void SetCurrentNotification(RequestNotification val)
		{
		}


		public override Exception Error { get { return _httpContext.Error; } }

		public void SetError(Exception val)
		{
		}

		public override IHttpHandler Handler
		{
			set
			{
				_httpContext.Handler = value;
			}
			get
			{
				return _httpContext.Handler;
			}
		}


		public override Boolean IsCustomErrorEnabled { get { return _httpContext.IsCustomErrorEnabled; } }

		public void SetIsCustomErrorEnabled(Boolean val)
		{
		}


		public override Boolean IsDebuggingEnabled { get { return _httpContext.IsDebuggingEnabled; } }

		public void SetIsDebuggingEnabled(Boolean val)
		{
		}


		public override Boolean IsPostNotification { get { return _httpContext.IsPostNotification; } }

		public void SetIsPostNotification(Boolean val)
		{
		}


		public override Boolean IsWebSocketRequest { get { return _httpContext.IsWebSocketRequest; } }

		public void SetIsWebSocketRequest(Boolean val)
		{
		}


		public override Boolean IsWebSocketRequestUpgrading { get { return _httpContext.IsWebSocketRequestUpgrading; } }

		public void SetIsWebSocketRequestUpgrading(Boolean val)
		{
		}

		public override IDictionary Items
		{
			get
			{
				return _httpContext.Items;
			}
		}

		public void SetItems(IDictionary val)
		{
			_httpContext.SetItems(val);
		}


		public override PageInstrumentationService PageInstrumentation { get { return _httpContext.PageInstrumentation; } }

		public void SetPageInstrumentation(PageInstrumentationService val)
		{
		}


		public override IHttpHandler PreviousHandler { get { return _httpContext.PreviousHandler; } }

		public void SetPreviousHandler(IHttpHandler val)
		{
		}


		public override ProfileBase Profile { get { return _httpContext.Profile; } }

		public void SetProfile(ProfileBase val)
		{
		}

		public override Boolean SkipAuthorization
		{
			set
			{
				_httpContext.SkipAuthorization = value;
			}
			get
			{
				return _httpContext.SkipAuthorization;
			}
		}


		public override DateTime Timestamp { get { return _httpContext.Timestamp; } }

		public void SetTimestamp(DateTime val)
		{
		}

		public override Boolean ThreadAbortOnTimeout
		{
			set
			{
				_httpContext.ThreadAbortOnTimeout = value;
			}
			get
			{
				return _httpContext.ThreadAbortOnTimeout;
			}
		}


		public override TraceContext Trace { get { return _httpContext.Trace; } }

		public void SetTrace(TraceContext val)
		{
		}

		public override IPrincipal User
		{
			set
			{
				_httpContext.User = value;
			}
			get
			{
				return _httpContext.User;
			}
		}


		public override String WebSocketNegotiatedProtocol { get { return _httpContext.WebSocketNegotiatedProtocol; } }

		public void SetWebSocketNegotiatedProtocol(String val)
		{
		}


		public override IList<String> WebSocketRequestedProtocols { get { return _httpContext.WebSocketRequestedProtocols; } }

		public void SetWebSocketRequestedProtocols(IList<String> val)
		{
		}
		public void InitializeUnsettable()
		{
			_application = _httpContext.Application;
			_server = _httpContext.Server;
			_session = _httpContext.Session;
			_request = new WrappedHttpRequest((IHttpRequest)_httpContext.Request);
			_response = new WrappedHttpResponse((IHttpResponse)_httpContext.Response);

			_response.ContentEncoding = _request.ContentEncoding;
		}

		private HttpApplicationStateBase _application;
		public override HttpApplicationStateBase Application { get { return _application; } }

		public void SetApplication(HttpApplicationStateBase val)
		{
		}

		private HttpRequestBase _request;
		private HttpResponseBase _response;
		public override HttpRequestBase Request { get { return _request; } }

		public void SetRequest(HttpRequestBase val)
		{
			_request = val;
		}


		public override HttpResponseBase Response { get { return _response; } }

		public void SetResponse(HttpResponseBase val)
		{
			_response = val;
		}

		private HttpServerUtilityBase _server;
		public override HttpServerUtilityBase Server { get { return _server; } }

		public void SetServer(HttpServerUtilityBase val)
		{
			_server = val;
		}

		private HttpSessionStateBase _session;
		public override HttpSessionStateBase Session { get { return _session; } }

		public void SetSession(HttpSessionStateBase val)
		{
			_session = val;
		}

		private HttpSessionStateBase ConvertSession(HttpSessionState session)
		{
			return new DefaultHttpSessionState(session);
		}

		// ReSharper disable once UnusedParameter.Local
		private HttpApplicationStateBase ConvertApplication(HttpApplicationState application)
		{
			return null;
		}

		// ReSharper disable once UnusedParameter.Local
		private HttpServerUtilityBase ConvertServer(HttpServerUtility server)
		{
			return null;
		}

		public Task InitializeWebSocket()
		{
			throw new NotImplementedException();
		}
	}
}