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
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.Utils
{
	public class InMemorySessionStorage : ISessionStorage
	{
		private const string IN_MEMORY_SESSION = "Node.Cs.Lib.Utils.InMemorySessionStorage";
		private readonly CoroutineMemoryCache _memoryCache;
		private readonly int _sessionTimeoutSeconds;

		public InMemorySessionStorage(CoroutineMemoryCache memoryCache, int sessionTimeoutSeconds)
		{
			RunId = Guid.NewGuid();
			_memoryCache = memoryCache;
			_sessionTimeoutSeconds = sessionTimeoutSeconds;
			// ReSharper disable once IteratorMethodResultIsIgnored
			_memoryCache.CreateArea(IN_MEMORY_SESSION, new TimeSpan(0, 0, 0, _sessionTimeoutSeconds));
		}

		public Guid RunId { get; private set; }
		public int SessionTimeoutSeconds { get { return _sessionTimeoutSeconds; } }

		public IEnumerable<Step> CreateSession(string sessionId, Dictionary<string, object> data = null)
		{
			data = data ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (SessionTimeoutSeconds <= 0)
			{
				yield return Step.DataStep(data);
				yield break;
			}
			foreach (var item in _memoryCache.AddOrGetValue(sessionId, data, IN_MEMORY_SESSION, TimeSpan.FromSeconds(_sessionTimeoutSeconds)))
			{
				if (item != null && item.Data != null)
				{
					var result = item.Data as Dictionary<string, object>;
					yield return Step.DataStep(result);
				}
				yield return null;
			}
		}


		public void StoreSession(string sessionId, Dictionary<string, object> data)
		{
			if (SessionTimeoutSeconds <= 0) return;
			if (data != null)
			{
				_memoryCache.AddOrReplaceValue(sessionId, data, IN_MEMORY_SESSION, TimeSpan.FromSeconds(_sessionTimeoutSeconds));
			}
		}

		public void ClearSession(string sessionId)
		{
			if (SessionTimeoutSeconds <= 0) return;
			_memoryCache.AddOrReplaceValue(sessionId, null, IN_MEMORY_SESSION, TimeSpan.FromMilliseconds(1));
		}
	}
}
