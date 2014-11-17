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
using System.Text;
using System.Web;
using Node.Cs.Lib;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Authorization
{
	public class Authorize : FilterBase
	{
		internal const string AUTH_COOKIE_ID = "NODECS:AC";

		public static void FormSetAuthCookie(HttpContextBase ctx, string userName, bool persistent)
		{
			ctx.Response.AppendCookie(new HttpCookie(AUTH_COOKIE_ID, userName)
			{
				Expires = DateTime.Now.AddMinutes(30)
			});
		}

		public static RedirectResponse BasicSignOut(HttpContextBase ctx)
		{
			var dataPath = NodeCsSettings.Settings.Listener.RootDir ?? string.Empty;
			var urlHelper = new UrlHelper(ctx);
			var realUrl = urlHelper.MergeUrl(dataPath);
			return new RedirectResponse(realUrl);
		}

		public static void FormSignOut(HttpContextBase ctx)
		{
			ctx.Response.SetCookie(new HttpCookie(AUTH_COOKIE_ID, "")
				{
					Expires = DateTime.Now.AddDays(-1)
				});
		}

		protected bool OnPreExecuteBasicAuthentication(HttpContextBase context, bool throwOnError = true)
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
					throw new NodeCsException("Invalid Basic Authentication header", 401);
				return false;
			}


			var basicData = Encoding.ASCII.GetString(
				Convert.FromBase64String(splittedEncoding[1].Trim()));
			var splitted = basicData.Split(':');
			if (splitted.Length != 2)
			{
				if (throwOnError)
					throw new NodeCsException("Invalid Basic Authentication data", 401);
				return false;
			}
			var authenticationDataProvider = GlobalVars.AuthenticationDataProvider;
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

		private bool RequireBasicAuthentication(HttpContextBase context)
		{
			var response = context.Response as IForcedHeadersResponse;
			if (response == null) return false;
			context.Response.StatusCode = 401;

			var realmDescription = string.Format("Basic realm=\"{0}\"", _realm);
			response.ForceHeader("WWW-Authenticate", realmDescription);
			return true;
		}

		private HttpCookie FormGetAuthCookie(HttpContextBase ctx)
		{
			return ctx.Request.Cookies.Get(AUTH_COOKIE_ID);
		}

		internal readonly string _realm;
		internal SecurityDefinition _settings;
		private string _roles;
		private string[] _rolesExploded;

		public Authorize()
		{
			_rolesExploded = new string[] { };
			_settings = NodeCsSettings.Settings.Security;
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

		protected bool OnPreExecuteFormAuthentication(HttpContextBase context, bool throwOnError = true)
		{
			var cookie = FormGetAuthCookie(context);
			if (cookie == null)
			{
				if (throwOnError)
					return RequireFormAuthentication(context);
				return false;
			}
			var authenticationDataProvider = GlobalVars.AuthenticationDataProvider;
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

		public override bool OnPreExecute(HttpContextBase context)
		{
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
		private bool RequireFormAuthentication(HttpContextBase context)
		{
			context.Response.Redirect(_settings.LoginPage);
			return true;
		}
	}
}
