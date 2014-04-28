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
using ConcurrencyHelpers.Coroutines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node.Cs.Lib.OnReceive;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Test.Bases;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Test
{
	/// <summary>
	/// Summary description for OnHttpListenerReceivedCoroutineTest
	/// </summary>
	[TestClass]
	public class OnHttpListenerReceivedCoroutineTest : BaseCoroutineTest
	{
		[TestMethod]
		public void ItShouldBePossibleToInvokeASessionIncapableView()
		{
			var ctxManager = new Mock<IContextManager>();
			var sesManager = new Mock<ISessionManager>();
			var nodeCsServer = new Mock<INodeCsServer>();
			var target = new OnHttpListenerReceivedCoroutine { InterceptLocalCalls = true };

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
															{
																return Interceptor(src, par);
															};
			
			target.Initialize(ctxManager.Object, sesManager.Object, nodeCsServer.Object,
				new Mock<IPagesManager>().Object);
			var enumerator = target.Run().GetEnumerator();
			var cycleCount = 0;
			while (enumerator.MoveNext())
			{
				cycleCount++;
				if (cycleCount > 2) Assert.AreEqual(2, cycleCount);
			}

			Assert.AreEqual(2, cycleCount);

			ctxManager.Verify(a => a.InitializeContext(), Times.Once);
			sesManager.Verify(a => a.InitializeSession(false, ctxManager.Object), Times.Once);
			sesManager.Verify(a => a.LoadSessionData(It.IsAny<Container>()), Times.Never);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(ViewsManagerCoroutine));

			CleanupListener();
		}


		[TestMethod]
		public void ItShouldBePossibleToInvokeAController()
		{
			var ctxManager = new Mock<IContextManager>();
			var sesManager = new Mock<ISessionManager>();
			var nodeCsServer = new Mock<INodeCsServer>();
			var target = new OnHttpListenerReceivedCoroutine { InterceptLocalCalls = true };
			var ri = new RouteInstance
							 {
								 Parameters = new Dictionary<string, object> { { "test", "test" } },
								 StaticRoute = false
							 };

			ctxManager.Setup(a => a.IsNotStaticRoute).Returns(true);
			ctxManager.Setup(a => a.RouteDefintion).Returns(ri);
			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			target.Initialize(ctxManager.Object, sesManager.Object, nodeCsServer.Object,
				new Mock<IPagesManager>().Object);
			var enumerator = target.Run().GetEnumerator();
			var cycleCount = 0;
			while (enumerator.MoveNext())
			{
				cycleCount++;
				if (cycleCount > 2) Assert.AreEqual(2, cycleCount);
			}

			Assert.AreEqual(2, cycleCount);

			ctxManager.Verify(a => a.InitializeContext(), Times.Once);
			sesManager.Verify(a => a.InitializeSession(false, ctxManager.Object), Times.Once);
			sesManager.Verify(a => a.LoadSessionData(It.IsAny<Container>()), Times.Never);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(ControllersManagerCoroutine));

			var cmc = (ControllersManagerCoroutine)Intercepted[0];
			Assert.AreEqual(ri.Parameters.Count, cmc.RouteInstanceParams.Count);
			Assert.IsTrue(cmc.RouteInstanceParams.ContainsKey("test"));
			Assert.AreEqual("test", cmc.RouteInstanceParams["test"]);

			CleanupListener();
		}

		[TestMethod]
		public void ItShouldBePossibleToInvokeASessionCapableView()
		{
			var ctxManager = new Mock<IContextManager>();
			var sesManager = new Mock<ISessionManager>();
			var nodeCsServer = new Mock<INodeCsServer>();
			var target = new OnHttpListenerReceivedCoroutine { InterceptLocalCalls = true };

			sesManager.Setup(a => a.SupportSession).Returns(true);
			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			target.Initialize(ctxManager.Object, sesManager.Object, nodeCsServer.Object,
				new Mock<IPagesManager>().Object);
			var enumerator = target.Run().GetEnumerator();
			var cycleCount = 0;
			while (enumerator.MoveNext())
			{
				cycleCount++;
				if (cycleCount > 3) Assert.AreEqual(3, cycleCount);
			}
			Assert.AreEqual(3, cycleCount);

			ctxManager.Verify(a => a.InitializeContext(), Times.Once);
			sesManager.Verify(a => a.InitializeSession(false, ctxManager.Object), Times.Once);
			sesManager.Verify(a => a.LoadSessionData(It.IsAny<Container>()), Times.Once);

			Assert.AreEqual(2, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(Func<IEnumerable<Step>>));
			Assert.IsInstanceOfType(Intercepted[1], typeof(ViewsManagerCoroutine));

			CleanupListener();
		}
	}
}
