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
using Node.Cs.Lib.OnReceive;

namespace Node.Cs.Lib.Utils
{
	public class OnHttpListenerReceivedCoroutine : Coroutine
	{
		protected virtual bool IsChildRequest { get { return false; } }

		protected IContextManager _contextManager;
		private ISessionManager _sessionManager;
		public ViewsManagerCoroutine ViewsManager { get; set; }
		public Dictionary<string, object> ViewData { get; set; }

		public override IEnumerable<Step> Run()
		{
			yield return _contextManager.InitializeContext();
			_contextManager.InitializeResponse();

			_sessionManager.InitializeSession(IsChildRequest, _contextManager);
			if (_sessionManager.SupportSession)
			{
				var actionResult = new Container();
				yield return InvokeLocalAndWait(() => GlobalVars.SessionStorage.CreateSession(_sessionManager.StoredSid), actionResult);
				_sessionManager.LoadSessionData(actionResult);
			}


			if (_contextManager.IsNotStaticRoute)
			{
				var controllersManager = new ControllersManagerCoroutine();
				controllersManager.PagesManager = _pagesManager;
				controllersManager.IsChildRequest = IsChildRequest;
				controllersManager.ViewData = ViewData;
				controllersManager.Context = _contextManager.Context;
				controllersManager.SessionManager = _sessionManager;
				controllersManager.RouteDefintion = _contextManager.RouteDefintion;
				controllersManager.InitializeRouteInstance();
				yield return InvokeCoroutineAndWait(controllersManager);
			}
			else
			{
				yield return InvokeCoroutineAndWait(BuildViewManager());
			}
			ShouldTerminate = true;
		}

		protected virtual ViewsManagerCoroutine BuildViewManager()
		{

			var viewsManager = new ViewsManagerCoroutine();
			viewsManager.PagesManager = _pagesManager;
			viewsManager.ViewData = ViewData;
			viewsManager.SessionManager = _sessionManager;
			viewsManager.IsChildRequest = IsChildRequest;

			viewsManager.LocalPath = _contextManager.LocalPath;
			viewsManager.ViewData = ViewData;
			viewsManager.IsChildRequest = IsChildRequest;
			viewsManager.Context = _contextManager.Context;
			
			ViewsManager = viewsManager;
			return viewsManager;
		}


		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			GlobalVars.ExceptionManager.HandleException(ex, (HttpContextBase)_contextManager.Context);
			_sessionManager.StoreContext(_contextManager.Context);
			_contextManager.Context.Response.Close();
		}

		private IPagesManager _pagesManager;

		public void Initialize(IContextManager contextManager, ISessionManager sessionManager,
			INodeCsServer nodeCsServer,IPagesManager pagesManager)
		{
			_contextManager = contextManager;
			_sessionManager = sessionManager;
			_pagesManager = pagesManager;
		}

		public OnHttpListenerReceivedCoroutine()
		{
			ViewData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}


	}


}
