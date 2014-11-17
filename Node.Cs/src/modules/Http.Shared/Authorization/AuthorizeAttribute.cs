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


using Http.Shared.Contexts;
using Http.Shared.Routing;
using HttpMvc.Controllers;
using Node.Cs.Authorization;
using NodeCs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Http.Shared.Authorization
{
    public static class SecurityUtils
    {
        public const string AUTH_COOKIE_ID = "NODECS:AC";

        public static void FormSetAuthCookie(IHttpContext ctx, string userName, bool persistent)
        {
            ctx.Response.AppendCookie(new HttpCookie(AUTH_COOKIE_ID, userName)
            {
                Expires = DateTime.Now.AddMinutes(30)
            });
        }

        /*
		public static RedirectResponse BasicSignOut(IHttpContext ctx)
		{
			var dataPath = NodeCsSettings.Settings.Listener.RootDir ?? string.Empty;
            var routeHandler = ServiceLocator.Locator.Resolve<IRoutingHandler>();
			var urlHelper = new UrlHelper(ctx,routeHandler);
			var realUrl = urlHelper.MergeUrl(dataPath);
			return new RedirectResponse(realUrl);
		}*/

        public static void FormSignOut(IHttpContext ctx)
        {
            ctx.Response.SetCookie(new HttpCookie(AUTH_COOKIE_ID, "")
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
        }
    }
    public class AuthorizeAttribute : Attribute, IFilter
    {

        protected bool OnPreExecuteBasicAuthentication(IHttpContext context, bool throwOnError = true)
        {
            var encodedAuthentication = context.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(encodedAuthentication))
            {
                return RequireBasicAuthentication(context);
            }
            var splittedEncoding = encodedAuthentication.Split(' ');
            if (splittedEncoding.Length != 2)
            {
                if (throwOnError)
                    throw new HttpException(401, "Invalid Basic Authentication header");
                return false;
            }


            var basicData = Encoding.ASCII.GetString(
                Convert.FromBase64String(splittedEncoding[1].Trim()));
            var splitted = basicData.Split(':');
            if (splitted.Length != 2)
            {
                if (throwOnError)
                    throw new HttpException(401, "Invalid Basic Authentication header");
                return false;
            }
            var authenticationDataProvider = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();
            if (!authenticationDataProvider.IsUserAuthorized(splitted[0], splitted[1]))
            {
                if (throwOnError)
                    return RequireBasicAuthentication(context);
                return false;
            }
            var userRoles = authenticationDataProvider.GetUserRoles(splitted[0]);
            if (userRoles == null) userRoles = new string[] { };
            context.User = new NodeCsPrincipal(new NodeCsIdentity(splitted[0], "basic", true), userRoles);
            return true;
        }

        private bool RequireBasicAuthentication(IHttpContext context)
        {
            var response = context.Response as IForcedHeadersResponse;
            if (response == null) return false;
            context.Response.StatusCode = 401;

            var realmDescription = string.Format("Basic realm=\"{0}\"", _realm);
            response.ForceHeader("WWW-Authenticate", realmDescription);
            return true;
        }

        private HttpCookie FormGetAuthCookie(IHttpContext ctx)
        {
            return ctx.Request.Cookies.Get(SecurityUtils.AUTH_COOKIE_ID);
        }

        internal readonly string _realm;
        internal SecurityDefinition _settings;
        private string _roles;
        private string[] _rolesExploded;

        public AuthorizeAttribute()
        {
            _rolesExploded = new string[] { };
            _settings = ServiceLocator.Locator.Resolve<SecurityDefinition>();
            _realm = _settings.Realm;
        }

        /// <summary>
        /// The roles allowed, comma separated
        /// </summary>
        public string Roles
        {
            get { return _roles; }
            set
            {
                _roles = value;
                _rolesExploded = _roles == null ? new string[] { } : _roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        protected bool OnPreExecuteFormAuthentication(IHttpContext context, bool throwOnError = true)
        {
            var cookie = FormGetAuthCookie(context);
            if (cookie == null)
            {
                if (throwOnError)
                    return RequireFormAuthentication(context);
                return false;
            }
            var authenticationDataProvider = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();
            var userName = cookie.Value;
            if (userName == null || !authenticationDataProvider.IsUserPresent(userName))
            {
                if (throwOnError)
                    return RequireFormAuthentication(context);
                return false;
            }
            var userRoles = authenticationDataProvider.GetUserRoles(userName);
            if (userRoles == null) userRoles = new string[] { };
            context.User = new NodeCsPrincipal(new NodeCsIdentity(userName, "basic", true), userRoles);
            return true;
        }

        public bool OnPreExecute(IHttpContext context)
        {
            if (string.Compare(_settings.AuthenticationType, "none", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (context.User == null)
            {
                if (string.Compare(_settings.AuthenticationType, "basic", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return OnPreExecuteBasicAuthentication(context);
                }
                if (string.Compare(_settings.AuthenticationType, "form", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return OnPreExecuteFormAuthentication(context);
                }
                return true;
            }
            for (int i = 0; i < _rolesExploded.Length; i++)
            {
                if (!context.User.IsInRole(_rolesExploded[i]))
                {
                    if (string.Compare(_settings.AuthenticationType, "basic", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return OnPreExecuteBasicAuthentication(context);
                    }
                    if (string.Compare(_settings.AuthenticationType, "form", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return OnPreExecuteFormAuthentication(context);
                    }
                }
            }

            return true;
        }
        private bool RequireFormAuthentication(IHttpContext context)
        {
            context.Response.Redirect(_settings.LoginPage);
            return true;
        }


        public void OnPostExecute(IHttpContext context)
        {

        }
    }
}
