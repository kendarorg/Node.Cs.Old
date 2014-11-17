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


using System.Threading.Tasks;
using System.Web.SessionState;
using System;
using System.Collections;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class FromBasetHttpSessionState : HttpSessionStateBase, IHttpSession
	{
		public FromBasetHttpSessionState() { }
		public object SourceObject { get { return _httpSessionStateBase; } }
		private readonly HttpSessionStateBase _httpSessionStateBase;

		public FromBasetHttpSessionState(HttpSessionStateBase httpSessionState)
		{
			_httpSessionStateBase = httpSessionState;
			InitializeUnsettable();
		}

		public void SetIsChanged(bool val)
		{
			_isChanged = false;
		}

		public override void Abandon()
		{
			_isChanged = true;
			_httpSessionStateBase.Abandon();
		}

		public override void Add(String name, Object value)
		{
			_isChanged = true;
			_httpSessionStateBase.Add(name, value);
		}

		public override void Clear()
		{
			_isChanged = true;
			_httpSessionStateBase.Clear();
		}

		public override void Remove(String name)
		{
			_isChanged = true;
			_httpSessionStateBase.Remove(name);
		}

		public override void RemoveAll()
		{
			_isChanged = true;
			_httpSessionStateBase.RemoveAll();
		}

		public override void RemoveAt(Int32 index)
		{
			_isChanged = true;
			_httpSessionStateBase.RemoveAt(index);
		}

		public override void CopyTo(Array array, Int32 index)
		{
			if (array != null)
			{
				_httpSessionStateBase.CopyTo(array, index);
			}
		}

		public override IEnumerator GetEnumerator()
		{
			return _httpSessionStateBase.GetEnumerator();
		}

		public override Int32 CodePage
		{
			set
			{
				_httpSessionStateBase.CodePage = value;
			}
			get
			{
				return _httpSessionStateBase.CodePage;
			}
		}

		public override HttpSessionStateBase Contents { get { return null; } }

		public void SetContents(HttpSessionStateBase val)
		{

		}


		public override HttpCookieMode CookieMode { get { return _httpSessionStateBase.CookieMode; } }

		public void SetCookieMode(HttpCookieMode val)
		{
		}


		public override Boolean IsCookieless { get { return _httpSessionStateBase.IsCookieless; } }

		public void SetIsCookieless(Boolean val)
		{
		}


		public override Boolean IsNewSession { get { return _httpSessionStateBase.IsNewSession; } }

		public void SetIsNewSession(Boolean val)
		{
		}


		public override Boolean IsReadOnly { get { return _httpSessionStateBase.IsReadOnly; } }

		public void SetIsReadOnly(Boolean val)
		{
		}


		public override NameObjectCollectionBase.KeysCollection Keys { get { return _httpSessionStateBase.Keys; } }

		public void SetKeys(NameObjectCollectionBase.KeysCollection val)
		{
		}

		public override Int32 LCID
		{
			set
			{
				_httpSessionStateBase.LCID = value;
			}
			get
			{
				return _httpSessionStateBase.LCID;
			}
		}


		public override SessionStateMode Mode { get { return _httpSessionStateBase.Mode; } }

		public void SetMode(SessionStateMode val)
		{
		}


		public override String SessionID { get { return _httpSessionStateBase.SessionID; } }

		public void SetSessionID(String val)
		{
		}


		public override HttpStaticObjectsCollectionBase StaticObjects { get { return null; } }

		public void SetStaticObjects(HttpStaticObjectsCollectionBase val)
		{
		}

		public override Int32 Timeout
		{
			set
			{
				_isChanged = true;
				_httpSessionStateBase.Timeout = value;
			}
			get
			{
				return _httpSessionStateBase.Timeout;
			}
		}

		public override object this[int index]
		{
			get { return _httpSessionStateBase[index]; }
			set
			{
				_isChanged = true;
				_httpSessionStateBase[index] = value;
			}
		}

		public override object this[string name]
		{
			get { return _httpSessionStateBase[name]; }
			set
			{
				_isChanged = true;
				_httpSessionStateBase[name] = value;
			}
		}


		public override Int32 Count { get { return _httpSessionStateBase.Count; } }

		public void SetCount(Int32 val)
		{
		}


		public override Boolean IsSynchronized { get { return _httpSessionStateBase.IsSynchronized; } }

		public void SetIsSynchronized(Boolean val)
		{
		}


		public override Object SyncRoot { get { return _httpSessionStateBase.SyncRoot; } }

		public void SetSyncRoot(Object val)
		{
		}
		public void InitializeUnsettable()
		{
		}

		private bool _isChanged;

		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public void Initialize(Dictionary<string, object> initVals)
		{
			foreach (var kvp in initVals)
			{
				_httpSessionStateBase.Add(kvp.Key, kvp.Value);
			}
		}

		public Dictionary<string, object> ItemDictionary
		{
			get
			{
				var result = new Dictionary<String, object>();
				foreach (string item in _httpSessionStateBase.Keys)
				{
					var val = _httpSessionStateBase[item];
					result.Add(item, val);
				}

				return result;
			}
		}

		public Task InitializeWebSocket()
		{
			throw new NotImplementedException();
		}
	}
}