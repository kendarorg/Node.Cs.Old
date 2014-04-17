using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Node.Cs.Lib.Contexts;

namespace Node.Cs.Lib.Test
{
	public class MockSessionState : HttpSessionStateBase,INodeCsSession
	{
		private string _sessionId;
		public MockSessionState(string sid)
		{
			_sessionId = sid;
			SessionData = new Dictionary<string, object>();
		}
		public Dictionary<string, object> SessionData { get; private set; }

		public override string SessionID
		{
			get { return _sessionId; }
		}

		public bool Changed;
		public bool Cleared;

		public bool IsChanged
		{
			get { return Changed; }
		}

		public bool IsCleared
		{
			get { return Cleared; }
		}
	}
	public class MockContext : HttpContextBase, INodeCsContext
	{
		private readonly HttpRequestBase _request;
		private readonly HttpResponseBase _response;
		private MockSessionState _session;

		public MockContext(HttpRequestBase request, HttpResponseBase response,MockSessionState session)
		{
			_request = request;
			_response = response;
			_session = session;
		}

		public override HttpSessionStateBase Session{ get { return _session; } }

		public override HttpRequestBase Request { get { return _request; } }
		public override HttpResponseBase Response { get { return _response; } }
		public Dictionary<string, object> RouteParams { get; set; }
		public Dictionary<string, object> SessionData { get;set; }

		public void Initialize(HttpListenerContext context, int sessionTimeoutSeconds, Guid runId)
		{
			
		}

		public void UpdateSessionCookie()
		{
			
		}
	}
}