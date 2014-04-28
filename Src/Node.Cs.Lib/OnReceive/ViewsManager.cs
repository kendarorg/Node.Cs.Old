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
using System.Dynamic;
using System.IO;
using System.Text;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Handlers;

namespace Node.Cs.Lib.OnReceive
{
	public class ViewsManagerCoroutine : Coroutine, IViewsManagerCoroutine
	{
		private IResourceHandler _resourceHandler;

		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			GlobalVars.ExceptionManager.HandleException(ex, (HttpContextBase)Context);
			Context.Response.Close();
		}

		protected void RunGlobalPreFilters()
		{
			if (IsChildRequest) return;
			foreach (var filter in GlobalVars.GlobalFilters)
			{
				filter.OnPreExecute((HttpContextBase)Context);
			}
		}

		protected void RunGlobalPostFilters(IResponse result)
		{
			if (IsChildRequest) return;
			foreach (var filter in GlobalVars.GlobalFilters)
			{
				filter.OnPostExecute((HttpContextBase)Context, result);
			}
		}

		public string LocalPath { get; set; }
		public INodeCsContext Context { get; set; }
		public dynamic ViewBag { get; set; }
		public IPagesManager PagesManager { get; set; }
		public object Model { get; set; }

		public ModelStateDictionary ModelState { get; set; }

		
		protected virtual Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			return InvokeCoroutineAndWait(GlobalVars.NodeCsServer.NextCoroutine, handlerInstance);
		}

		public override IEnumerable<Step> Run()
		{
			RunGlobalPreFilters();
			Context = Context;
			if (ViewBag == null)
			{
				ViewBag = new ExpandoObject();
			}


			if (ModelState == null) ModelState = new ModelStateDictionary();
			if (!Path.HasExtension(LocalPath))
			{
				foreach (var ext in GlobalVars.ExtensionHandler.Extensions)
				{
					var newPath = LocalPath + ext;
					var tmpReal = PagesManager.GetFilePath(newPath);
					if (tmpReal != null)
					{
						LocalPath += ext;
						break;
					}
				}
			}
			var foundedPath = PagesManager.GetFilePath(LocalPath);
			if (foundedPath == null)
			{
				throw new NodeCsException("Resource '{0}' not found.", 404, LocalPath);
			}

			var handlerInstance = GlobalVars.ExtensionHandler.CreateInstance((HttpContextBase)Context, foundedPath,IsChildRequest);
			_resourceHandler = handlerInstance as IResourceHandler;
			if (_resourceHandler == null)
			{
				yield return Step.Current;
			}

			_resourceHandler.StringBuilder = StringBuilder;
			_resourceHandler.Model = Model;
			_resourceHandler.ViewBag = ViewBag;
			_resourceHandler.ModelState = ModelState;
			_resourceHandler.ViewData = ViewData;


			yield return CallHandlerInstance(handlerInstance);

			RunGlobalPostFilters(null);
			if (!IsChildRequest)
			{
				SessionManager.StoreContext(Context);
			}
			yield break;
		}
		//public ContextManager ContextManager { get; set; }

		public Dictionary<string, object> ViewData { get; set; }

		public bool IsChildRequest { get; set; }

		public ISessionManager SessionManager { get; set; }

		public StringBuilder StringBuilder { get; set; }
	}
}
