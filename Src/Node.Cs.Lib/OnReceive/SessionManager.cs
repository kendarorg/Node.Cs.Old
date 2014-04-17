using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;

namespace Node.Cs.Lib.OnReceive
{
	public class SessionManager
	{
		private ContextManager _contextManager;
		public string StoredSid { get; private set; }
		public bool SupportSession { get; private set; }

		public virtual void InitializeSession(bool isChildRequest, ContextManager contextManager)
		{
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
					_contextManager.Context.SessionData[kvp.Key]=kvp.Value;
				}
			}
		}
	}
}
