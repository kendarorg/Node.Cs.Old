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
using System.Runtime.Caching;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.Utils
{
	public class MemoryCacheSessionStorage:ISessionStorage
	{
		public MemoryCacheSessionStorage(int sessionTimeoutSeconds)
		{
			SessionTimeoutSeconds = sessionTimeoutSeconds;
		}

		private readonly MemoryCache _memoryCache = new MemoryCache("M");
		public Guid RunId { get; private set; }
		public int SessionTimeoutSeconds { get; private set; }

		public IEnumerable<Step> CreateSession(string sessionId, Dictionary<string, object> data = null)
		{
			data = data ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			yield return Step.DataStep(_memoryCache.AddOrGetExisting(sessionId, data,DateTime.Now+new TimeSpan(0,0,SessionTimeoutSeconds)));
		}

		public void StoreSession(string sessionId, Dictionary<string, object> data = null)
		{
			data = data ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			_memoryCache.Add(sessionId, data, DateTime.Now + new TimeSpan(0, 0, SessionTimeoutSeconds));
		}

		public void ClearSession(string sessionId)
		{
			_memoryCache.Remove(sessionId);
		}
	}
}
