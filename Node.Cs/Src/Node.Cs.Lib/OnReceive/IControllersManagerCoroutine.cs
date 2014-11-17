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


using System.Collections.Generic;
using System.Text;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Lib.OnReceive
{
	public interface IControllersManagerCoroutine
	{
		bool IsChildRequest { get; set; }
		INodeCsContext Context { get; set; }
		RouteInstance RouteDefintion { get; set; }
		string LocalPath { get; set; }
		Dictionary<string, object> ViewData { get; set; }
		ISessionManager SessionManager { get; set; }
		string ActionName { get; set; }
		string ControllerName { get; set; }
		Dictionary<string, object> RouteInstanceParams { get; set; }
		StringBuilder StringBuilder { get; set; }

		IEnumerable<Step> HandleRoutedRequests(RouteInstance routeInstance, string localPath,
			INodeCsContext context, bool isChildRequest = false);

		void InitializeRouteInstance();
		Step InvokeCoroutineAndWait(ICoroutine action, bool adding);
	}
}