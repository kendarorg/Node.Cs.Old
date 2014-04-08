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

namespace Node.Cs.Lib.Contexts
{
	public class NodeCsHttpSession : HttpSessionStateBase
	{
		private readonly Dictionary<string, object> _sessionData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private string _sessionId;
		private readonly int _timeoutMs;
		private bool _isNew;
		private bool _isChanged;

		public override string SessionID
		{
			get { return _sessionId; }
		}

		internal Dictionary<string, object> SessionData
		{
			get
			{
				return _sessionData;
			}
		}

		public NodeCsHttpSession(string sessionId, int timeoutMs, bool isNew)
		{
			_sessionId = sessionId;
			_timeoutMs = timeoutMs;
			_isNew = isNew;
		}

		public override bool IsCookieless
		{
			get { return false; }
		}

		public override HttpCookieMode CookieMode
		{
			get { return HttpCookieMode.UseCookies; }
		}

		public override int Timeout { get { return _timeoutMs; } set { } }

		public override bool IsNewSession
		{
			get { return _isNew; }
		}

		public override void Add(string name, object value)
		{
			_isChanged = true;
			if (!_sessionData.ContainsKey(name))
			{
				_sessionData.Add(name, value);
			}
			else
			{
				_sessionData[name] = value;
			}
			SetupHasData();
		}

		private void SetupHasData()
		{
			if (_sessionData.Count > 0)
			{
				_sessionId = NodeCsContext.SESSION_FULL + _sessionId.Substring(1);
			}
			else
			{
				_sessionId = NodeCsContext.SESSION_EMPTY + _sessionId.Substring(1);
			}
		}

		public bool _cleared = false;
		public override void Clear()
		{
			_cleared = true;
			_sessionData.Clear();
			SetupHasData();
		}

		public bool IsCleared
		{
			get
			{
				return _cleared;
			}
		}

		public bool IsChanged
		{
			get
			{
				return _isChanged;
			}
		}

		public override int Count
		{
			get { return _sessionData.Count; }
		}

		public override void Remove(string name)
		{
			if (_sessionData.ContainsKey(name))
			{
				_isChanged = true;
				_sessionData.Remove(name);
				SetupHasData();
			}
		}

		public override object this[string name]
		{
			get
			{
				if (!_sessionData.ContainsKey(name)) return null;
				return _sessionData[name];
			}
			set
			{
				Add(name, value);
			}
		}

		public override void RemoveAll()
		{
			Clear();
		}

		public void SetNew(bool isNew)
		{
			_isNew = false;
		}
	}
}
