using CoroutinesLib.Shared;
using Http.Shared.Contexts;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Shared
{
    public static class SessionConstants
    {
        public const string SessionId = "NODE.SID";
    }
    public interface ISessionManager
    {
        IEnumerable<ICoroutineResult> InitializeSession(IHttpContext context);

        void SetCachingEngine(ICacheEngine cacheEngine);
    }

    public interface ISessionManagerFactory
    {
        ISessionManager CreateSessionManager();
    }
}
