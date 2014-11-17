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
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Web.SessionState;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class SimpleHttpSessionState : HttpSessionStateBase, IHttpSession
	{
		public override void Abandon()
		{
			_isChanged = true;
		}

		public void SetIsChanged(bool val)
		{
			_isChanged = false;
		}

		public override void Add(String name, Object value)
		{
			_isChanged = true;
			_item[name] = value;
		}

		public override void Clear()
		{
			_isChanged = true;
			_item.Clear();
		}

		public override void Remove(String name)
		{
			_isChanged = true;
			_item.Remove(name);
		}

		public override void RemoveAll()
		{
			_isChanged = true;
			_item.Clear();
		}

		public override void RemoveAt(Int32 index)
		{
			var key = _item.Keys.ToArray()[index];
			_isChanged = true;
			_item.Remove(key);
		}

		public override void CopyTo(Array array, Int32 index)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator GetEnumerator()
		{
			return null;
		}
		public override Int32 CodePage { get; set; }

		private HttpSessionStateBase _contents;

		public override HttpSessionStateBase Contents { get { return _contents; } }

		public void SetContents(HttpSessionStateBase val)
		{
			_contents = val;
		}

		private HttpCookieMode _cookieMode;

		public override HttpCookieMode CookieMode { get { return _cookieMode; } }

		public void SetCookieMode(HttpCookieMode val)
		{
			_cookieMode = val;
		}

		private Boolean _isCookieless;

		public override Boolean IsCookieless { get { return _isCookieless; } }

		public void SetIsCookieless(Boolean val)
		{
			_isCookieless = val;
		}

		private Boolean _isNewSession;

		public override Boolean IsNewSession { get { return _isNewSession; } }

		public void SetIsNewSession(Boolean val)
		{
			_isNewSession = val;
		}

		private Boolean _isReadOnly;

		public override Boolean IsReadOnly { get { return _isReadOnly; } }

		public void SetIsReadOnly(Boolean val)
		{
			_isReadOnly = val;
		}

		public override NameObjectCollectionBase.KeysCollection Keys { get { throw new NotImplementedException(); } }


		public override Int32 LCID { get; set; }

		private SessionStateMode _mode;

		public override SessionStateMode Mode { get { return _mode; } }

		public void SetMode(SessionStateMode val)
		{
			_mode = val;
		}

		private String _sessionID = "";

		public override String SessionID { get { return _sessionID; } }

		public void SetSessionID(String val)
		{
			_sessionID = val;
		}

		private HttpStaticObjectsCollectionBase _staticObjects;

		public override HttpStaticObjectsCollectionBase StaticObjects { get { return _staticObjects; } }

		public void SetStaticObjects(HttpStaticObjectsCollectionBase val)
		{
			_staticObjects = val;
		}

		public override Int32 Timeout { get; set; }

		private readonly Dictionary<string, object> _item = new Dictionary<string, object>();

		public override object this[string key]
		{
			get
			{
				if (!_item.ContainsKey(key)) return null;
				return _item[key];
			}
			set
			{
				_isChanged = true;
				_item[key] = value;
			}
		}
		public override object this[int key]
		{
			get
			{
				if (_item.Count() >= key) return null;
				return _item.Keys.ToArray()[key];
			}
		}

		public Dictionary<string, object> ItemDictionary
		{
			get { return new Dictionary<string, object>(_item); }
		}

		public override Int32 Count { get { return _item.Count; } }

		public void SetCount(Int32 val)
		{

		}

		private Boolean _isSynchronized;

		public override Boolean IsSynchronized { get { return _isSynchronized; } }

		public void SetIsSynchronized(Boolean val)
		{
			_isSynchronized = val;
		}

		private Object _syncRoot = new Object();

		public override Object SyncRoot { get { return _syncRoot; } }

		public void SetSyncRoot(Object val)
		{
			_syncRoot = val;
		}
		private bool _isChanged;
		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public void Initialize(System.Collections.Generic.Dictionary<string, object> initVals)
		{
			foreach (var kvp in initVals)
			{
				_item.Add(kvp.Key, kvp.Value);
			}
			_isChanged = false;
		}
	}
}