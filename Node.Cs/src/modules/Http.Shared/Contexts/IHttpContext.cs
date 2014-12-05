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
using System.Globalization;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Instrumentation;
using System.Web.Profile;
using System.Web.SessionState;
using System.Web.WebSockets;

namespace Http.Shared.Contexts
{

    public interface IForcedHeadersResponse
    {
        void SetContentLength(int p);
        void ForceHeader(string key, string value);
    }
	public interface IHttpContext 
	{
		IHttpContext RootContext { get; }
		IHttpContext Parent { get; }
		string RootDir { get; }
		void ForceRootDir(string rootDir);
		void ForceHeader(string key, string value);
		ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback);
		void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc);
		void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options);
		void AddError(Exception errorInfo);
		void ClearError();
		ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target);
		Object GetGlobalResourceObject(String classKey, String resourceKey);
		Object GetGlobalResourceObject(String classKey, String resourceKey, CultureInfo culture);
		Object GetLocalResourceObject(String virtualPath, String resourceKey);
		Object GetLocalResourceObject(String virtualPath, String resourceKey, CultureInfo culture);
		Object GetSection(String sectionName);
		void RemapHandler(IHttpHandler handler);
		void RewritePath(String path);
		void RewritePath(String path, Boolean rebaseClientPath);
		void RewritePath(String filePath, String pathInfo, String queryString);
		void RewritePath(String filePath, String pathInfo, String queryString, Boolean setClientFilePath);
		void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior);
		Object GetService(Type serviceType);
		Exception[] AllErrors { get; }
		Boolean AllowAsyncDuringSyncStages { get; set; }
		HttpApplicationStateBase Application { get; }
		HttpApplication ApplicationInstance { get; set; }
		AsyncPreloadModeFlags AsyncPreloadMode { get; set; }
		Cache Cache { get; }
		IHttpHandler CurrentHandler { get; }
		RequestNotification CurrentNotification { get; }
		Exception Error { get; }
		IHttpHandler Handler { get; set; }
		Boolean IsCustomErrorEnabled { get; }
		Boolean IsDebuggingEnabled { get; }
		Boolean IsPostNotification { get; }
		Boolean IsWebSocketRequest { get; }
		Boolean IsWebSocketRequestUpgrading { get; }
		IDictionary Items { get; }
		PageInstrumentationService PageInstrumentation { get; }
		IHttpHandler PreviousHandler { get; }
		ProfileBase Profile { get; }
		HttpRequestBase Request { get; }
		HttpResponseBase Response { get; }
		HttpServerUtilityBase Server { get; }
		HttpSessionStateBase Session { get; }
		Boolean SkipAuthorization { get; set; }
		DateTime Timestamp { get; }
		Boolean ThreadAbortOnTimeout { get; set; }
		TraceContext Trace { get; }
		IPrincipal User { get; set; }
		String WebSocketNegotiatedProtocol { get; }
		IList<String> WebSocketRequestedProtocols { get; }
		void SetAllErrors(Exception[] val);
		void SetApplication(HttpApplicationStateBase val);
		void SetCache(Cache val);
		void SetCurrentHandler(IHttpHandler val);
		void SetCurrentNotification(RequestNotification val);
		void SetError(Exception val);
		void SetIsCustomErrorEnabled(Boolean val);
		void SetIsDebuggingEnabled(Boolean val);
		void SetIsPostNotification(Boolean val);
		void SetIsWebSocketRequest(Boolean val);
		void SetIsWebSocketRequestUpgrading(Boolean val);
		void SetItems(IDictionary val);
		void SetPageInstrumentation(PageInstrumentationService val);
		void SetPreviousHandler(IHttpHandler val);
		void SetProfile(ProfileBase val);
		void SetRequest(HttpRequestBase val);
		void SetResponse(HttpResponseBase val);
		void SetServer(HttpServerUtilityBase val);
		void SetSession(HttpSessionStateBase val);
		void SetTimestamp(DateTime val);
		void SetTrace(TraceContext val);
		void SetWebSocketNegotiatedProtocol(String val);
		void SetWebSocketRequestedProtocols(IList<String> val);
		Task InitializeWebSocket();
		Dictionary<string, object> RouteParams { get; set; }
	}
}