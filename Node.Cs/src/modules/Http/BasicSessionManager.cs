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


using CoroutinesLib.Shared;
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Generic;

namespace Http
{
	public class BasicSessionManager : ISessionManager
	{
		private ICacheEngine _cacheEngine;

		public void SaveSession(IHttpContext context)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ICoroutineResult> InitializeSession(IHttpContext context)
		{
			if (_cacheEngine == null) yield break;
			if (context.Session != null) yield break;

			var newSession = new SimpleHttpSessionState();
			var cookie = context.Request.Cookies[SessionConstants.SessionId];
			if (cookie == null)
			{
				var nodecsSid = Guid.NewGuid().ToString();
				cookie = new System.Web.HttpCookie(SessionConstants.SessionId, nodecsSid);
				context.Response.SetCookie(cookie);
			}
			newSession.SetSessionID(cookie.Value);
			var realSession = newSession;
			yield return _cacheEngine.AddAndGet(new CacheDefinition
			{
				Value = newSession,
			}, (a) => realSession = (SimpleHttpSessionState)a, "basicSessionManager");
			context.SetSession(realSession);
		}

		public void SetCachingEngine(ICacheEngine cacheEngine)
		{
			_cacheEngine = cacheEngine;
			_cacheEngine.AddGroup(new CacheGroupDefinition
			{
				ExpireAfter = TimeSpan.FromMinutes(20),
				Id = "basicSessionManager",
				RollingExpiration = true
			});
		}
	}
}
