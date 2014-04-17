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
using System.Net;
using System.Security.Principal;
using System.Web;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.Contexts
{
	public class NodeCsContext : HttpContextBase, INodeCsContext
	{
		public Dictionary<string, object> Data = new Dictionary<string, object>();
		private HttpListenerContext _context;
		private NodeCsRequest _request;
		private NodeCsResponse _response;
		private IPrincipal _user;
		private NodeCsHttpSession _session;
		private Guid _runGuid;

		public const char SESSION_EMPTY = 'E';
		public const char SESSION_FULL = 'F';
		public const string NODECS_SESSIONID = "NODECS:SID";

		public NodeCsContext(HttpListenerContext context, int sessionTimeout, Guid runGuid)
		{
			Initialize(context, sessionTimeout, runGuid);
		}

		public NodeCsContext()
		{
		}

		public HttpListenerContext ListenerContext { get { return _context; } }
		public override HttpRequestBase Request { get { return _request; } }
		public override HttpResponseBase Response { get { return _response; } }

		public override IPrincipal User
		{
			get
			{
				return _user;
			}
			set
			{
				_user = value;
			}
		}

		private int _sessionTimeout;


		public void Initialize(HttpListenerContext context, int sessionTimeout, Guid runGuid)
		{
			_sessionTimeout = sessionTimeout;
			_runGuid = runGuid;
			_context = context;
			_user = _context.User;
			_response = new NodeCsResponse(_context.Response);
			_request = new NodeCsRequest(_context.Request, this);
			if (_context.Request.ContentType != null)
			{
				_response.ContentType = _context.Request.ContentType;
			}
			if (_context.Request.AcceptTypes != null && _context.Request.AcceptTypes.Length > 1)
			{
				_response.ContentType = _context.Request.AcceptTypes[0];
			}
			//TODO: Chunked _context.Response.SendChunked = false;

			var sessionCookie = _request.Cookies[NODECS_SESSIONID];

			bool isNew = false;
			if (sessionTimeout <= 0)
			{
				_session = new NodeCsHttpSession("E"+Guid.NewGuid().ToString(), sessionTimeout, isNew);
				return;
			}
			if (sessionCookie == null)
			{
				isNew = true;
				var sessionId = SESSION_EMPTY + Guid.NewGuid().ToString();
				sessionCookie = new HttpCookie(NODECS_SESSIONID, sessionId)
										 {
											 Path = "/",
											 Expires = DateTime.Now.AddSeconds(sessionTimeout)
										 };
				_response.AppendCookie(sessionCookie);
			}

			_session = new NodeCsHttpSession(sessionCookie.Value, sessionTimeout, isNew);

		}

		public bool SessionContainsData
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_session.SessionID)) return false;
				return _session.SessionID[0] == SESSION_FULL;
			}
		}

		public Dictionary<string, object> SessionData
		{
			get
			{
				return _session.SessionData;
			}
		}

		public override HttpSessionStateBase Session
		{
			get { return _session; }
		}

		public Dictionary<string, object> RouteParams { get; set; }

		public void UpdateSessionCookie()
		{
			if (_sessionTimeout <= 0) return;
			var sessionCookie = new HttpCookie(NODECS_SESSIONID, _session.SessionID)
			{
				Path = "/",
				Expires = DateTime.Now.AddSeconds(Session.Timeout)
			};
			_response.AppendCookie(sessionCookie);
		}
	}
}
