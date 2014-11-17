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
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Loggers;

namespace Node.Cs.Lib.OnReceive
{
	public class SessionManager : ISessionManager
	{
		private IContextManager _contextManager;
		public string StoredSid { get; private set; }
		public bool SupportSession { get; private set; }



		public void StoreContext(INodeCsContext context, bool disconnect = true)
		{
			if (!SupportSession) return;
			if (IsChildRequest) return;
			try
			{
				foreach (var filter in GlobalVars.GlobalFilters)
				{
					filter.OnPostExecute((HttpContextBase)context, null);
				}

				if (context.Session == null) throw new NullReferenceException("Missing session.");
				var session = (INodeCsSession)context.Session;
				if (session.IsCleared || session.IsChanged)
				{
					var storedSid = context.Session.SessionID.Substring(1);
					GlobalVars.SessionStorage.StoreSession(storedSid, context.SessionData);
					context.UpdateSessionCookie();
				}
				else if (session.IsCleared)
				{
					var storedSid = context.Session.SessionID.Substring(1);
					GlobalVars.SessionStorage.ClearSession(storedSid);
					context.UpdateSessionCookie();
				}
			}
			catch (Exception ex)
			{
				Logger.Warn("Problem storing session\r\n{0}", ex);
			}
			try
			{
				if (disconnect) context.Response.Close();
			}
			catch (Exception ex)
			{
			}
		}

		public virtual void InitializeSession(bool isChildRequest, IContextManager contextManager)
		{
			IsChildRequest = isChildRequest;
			_contextManager = contextManager;
			if (_contextManager.Context.Session == null) throw new NullReferenceException("Missing session.");
			var session = (INodeCsSession)_contextManager.Context.Session;
			//Retrieve the storage SessionID
			StoredSid = session.SessionID.Substring(1);
			if (_contextManager.Context.RouteParams.ContainsKey("controller"))
			{
				var controllerName = _contextManager.Context.RouteParams["controller"];
				SupportSession = GlobalVars.ControllersFactoryHandler.SupportSession(controllerName + "Controller");
			}
			else
			{
				SupportSession = !isChildRequest;
			}
		}

		public void LoadSessionData(Container actionResult)
		{
			var dataDescriptor = actionResult.RawData as Dictionary<string, object>;
			if (dataDescriptor != null)
			{
				foreach (var kvp in dataDescriptor)
				{
					_contextManager.Context.SessionData[kvp.Key] = kvp.Value;
				}
			}
		}

		public bool IsChildRequest { get; set; }
	}
}
