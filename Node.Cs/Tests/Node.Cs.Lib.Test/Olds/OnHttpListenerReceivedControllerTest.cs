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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Static;
using Node.Cs.Lib.Test.Bases;
using Node.Cs.Lib.Test.Mocks;

namespace Node.Cs.Lib.Test
{
#if NOPE
	[TestClass]
	public class OnHttpListenerReceivedControllerTest : OnHttpListenerReceivedTestBase
	{
		[TestMethod]
		public void ItShouldBePossibleToInvokeAController()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test.html", "testContent");

			var coThread = new CoroutineTestThread();

			var filter = new Mock<MockFilter>();
			GlobalVars.GlobalFilters.Add(filter.Object);

			var rhanlder = new Mock<IResponseHandler>();
			_responseHandler.Setup(a => a.Load(It.IsAny<Type>())).Returns(rhanlder.Object);

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>(),false))
				.Returns(new MockHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance
				{
					Parameters = new Dictionary<string, object> { { "controller", "controller" }, { "action", "action" } }
				});

			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.InvokeControllerAndWaitAction = a => a.RawData = new ViewResponse("test.html", new object());

			res.Initialize(null,null,_nodeCsServer.Object);

			foreach (var item in res.Run())
			{
				Console.Write("."+item);
			}
			filter.Verify(a => a.OnPreExecute(It.IsAny<HttpContextBase>()), Times.Once);
			filter.Verify(a => a.OnPostExecute(It.IsAny<HttpContextBase>(), It.IsAny<IResponse>()), Times.Once);
		}
	}
#endif
}
