using CoroutinesLib.Shared;
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using NodeCs.Shared;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class BasicSessionManager:ISessionManager
    {
        private ICacheEngine _cacheEngine;

        public IEnumerable<ICoroutineResult> InitializeSession(IHttpContext context)
        {
            if (_cacheEngine == null) yield break;
            if(context.Session !=null) yield break;

            var newSession = new SimpleHttpSessionState();
            var cookie = context.Request.Cookies[SessionConstants.SessionId];
            if(cookie==null){
                var nodecsSid = Guid.NewGuid().ToString();
                cookie = new System.Web.HttpCookie(SessionConstants.SessionId,nodecsSid);
                context.Response.SetCookie(cookie);
            }
            newSession.SetSessionID(cookie.Value);
            var realSession = newSession;
            yield return _cacheEngine.AddAndGet(new CacheDefinition
            {
                Value = newSession,
            }, (a) => realSession = (SimpleHttpSessionState) a, "basicSessionManager");
            context.SetSession(realSession);
        }

        public void SetCachingEngine(ICacheEngine cacheEngine)
        {
            _cacheEngine = cacheEngine;
            _cacheEngine.AddGroup( new CacheGroupDefinition{
                ExpireAfter = TimeSpan.FromMinutes(20),
                Id = "basicSessionManager",
                RollingExpiration = true
            });
        }
    }
}
