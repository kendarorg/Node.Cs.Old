using CoroutinesLib.Shared;
using Http.Shared.Contexts;
using NodeCs.Shared.Caching;
using System.Collections.Generic;

namespace Http.Shared
{
	public static class SessionConstants
	{
		public const string SessionId = "NODE.SID";
	}
	public interface ISessionManager
	{
		IEnumerable<ICoroutineResult> InitializeSession(IHttpContext context);
		void SaveSession(IHttpContext context);
		void SetCachingEngine(ICacheEngine cacheEngine);
	}

	public interface ISessionManagerFactory
	{
		ISessionManager CreateSessionManager();
	}
}
