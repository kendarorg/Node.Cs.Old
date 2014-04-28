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
using System.Net;
using System.Web;
using Node.Cs.Lib.Contexts;

namespace Node.Cs.Lib.Test.Mocks
{
	public class MockSessionState : HttpSessionStateBase,INodeCsSession
	{
		private readonly string _sessionId;
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
		private readonly MockSessionState _session;

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