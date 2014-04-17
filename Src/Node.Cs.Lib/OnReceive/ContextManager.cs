using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.ForTest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Lib.OnReceive
{
	public class ContextManager 
	{
		readonly IListenerContainer _listener;
		public RouteInstance RouteDefintion { get; private set; }
		public CultureInfo ListenerCulture { get; private set; }
		public string LocalPath { get; private set; }
		public Uri LocalUrl { get; private set; }
		public INodeCsContext Context { get; private set; }

		public ContextManager(IListenerContainer listener)
		{
			_listener = listener;
		}

		protected virtual HttpContextBase AssignContext(HttpContextBase context)
		{
			return context;
		}

		public virtual Step InitializeContext()
		{
			if (_listener == null)
			{
				return Step.Current;
			}

			InitializeCulture();
			Context = new NodeCsContext();
			return Coroutine.InvokeAsTaskAndWait(() => Context.Initialize((HttpListenerContext)_listener.Context,
				GlobalVars.SessionStorage.SessionTimeoutSeconds,
				GlobalVars.SessionStorage.RunId));
		}

		protected void InitializeCulture()
		{
			if (_listener == null)
			{
				return;
			}

			if (_listener.HasUserLanguage)
			{
				var userLanguages = _listener.UserLanguages;
				var langAvailable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var userLanguage in userLanguages)
				{
					var lang = userLanguage.Split(';')[0];
					langAvailable.Add(lang);
				}
				foreach (var lan in GlobalVars.Settings.Listener.Cultures.AvailableCultures)
				{
					var lk = lan.Key;
					if (langAvailable.Contains(lk))
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = lan.Value;
						ListenerCulture = lan.Value;
						return;
					}
					lk = lk.Substring(0, 2);
					if (langAvailable.Contains(lk))
					{
						System.Threading.Thread.CurrentThread.CurrentCulture = lan.Value;
						ListenerCulture = lan.Value;
						return;
					}
				}
			}
			System.Threading.Thread.CurrentThread.CurrentCulture = GlobalVars.Settings.Listener.Cultures.DefaultCulture;
			ListenerCulture = GlobalVars.Settings.Listener.Cultures.DefaultCulture;
		}

		public void InitializeResponse()
		{
			Context = (INodeCsContext)AssignContext((HttpContextBase)Context);

			// ReSharper disable once UnusedVariable
			var response = Context.Response;
			var request = Context.Request;
			// ReSharper disable once UnusedVariable
			LocalUrl = request.Url;
			// ReSharper disable once PossibleNullReferenceException
			LocalPath = LocalUrl.LocalPath.Trim();

			RouteDefintion = GlobalVars.RoutingService.Resolve(LocalPath, (HttpContextBase)Context);
			//var somethingHappened = false;
			if (RouteDefintion != null)
			{
				Context.RouteParams = RouteDefintion.Parameters ?? new Dictionary<string, object>();
			}
		}

		public bool IsNotStaticRoute
		{
			get
			{
				return RouteDefintion != null && !RouteDefintion.StaticRoute &&
						 (RouteDefintion.Parameters.ContainsKey("controller") && RouteDefintion.Parameters.ContainsKey("action"));
			}
		}

		
	}
}
