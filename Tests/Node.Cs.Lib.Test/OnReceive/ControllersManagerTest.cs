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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.OnReceive;
using Node.Cs.Lib.Test.Mocks;
using Moq;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Test.Bases;
using ConcurrencyHelpers.Coroutines;
using System.Web;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Utils;
using System.IO;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Filters;

namespace Node.Cs.Lib.Test.OnReceive
{
	/// <summary>
	/// Summary description for ControllersManagerTest
	/// </summary>
	[TestClass]
	public class ControllersManagerTest : BaseCoroutineTest
	{
		public List<Exception> Exceptions = new List<Exception>();
		private static MockFilter _globalFilter;
		private MockResponse _response;
		private CoroutineTestThread _testCoroutine;

		[ClassInitialize]
		public static void ClassInitialize(TestContext ctx)
		{
			_globalFilter = new MockFilter();
			GlobalVars.GlobalFilters = new List<FilterBase> { _globalFilter };
			GlobalVars.ExtensionHandler = new MockExtensionHandler();
		}

		[TestInitialize]
		public void TestInitialize()
		{
			_globalFilter.CountPreExecute = 0;
			_globalFilter.CountPostExecute = 0;
			_testCoroutine = new CoroutineTestThread();
			_testCoroutine.Initialize();

			InitializeListener();
		}


		private void CallCoroutinesWithControllers(RouteInstance ri, int maxCycles, Action actionBeforeRun = null, bool childRequest = false)
		{

			Exceptions.Clear();
			var tw = new StringWriter();
			_response = new MockResponse(new HttpResponse(tw));
			var request = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", ""));
			var ctx = new MockContext(request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			GlobalVars.ExceptionManager = new Mock<IGlobalExceptionManager>().Object;
			GlobalVars.ConversionService = new Mock<IConversionService>().Object;

			GlobalVars.ResponseHandlers = new Mock<IResponseHandlersFactory>().Object;

			GlobalVars.ControllersFactoryHandler = new ControllersFactoryHandler();
			GlobalVars.ControllersFactoryHandler.Initialize("Node.Cs.Lib.Controllers.BasicControllersFactory");
			var sess = new Mock<ISessionManager>();

			if (actionBeforeRun != null) actionBeforeRun();

			var target = new ControllersManagerCoroutine();
			target.IsChildRequest = childRequest;
			target.ViewData = null;
			target.Context = ctx;
			target.SessionManager = sess.Object;
			target.RouteDefintion = ri;
			target.InitializeRouteInstance();


			_testCoroutine.AddCoroutine(target);
			_testCoroutine.HandleError = (ex, cr) =>
			{
				Exceptions.Add(ex);
			};

			var cycleCount = 0;
			while (cycleCount < maxCycles)
			{
				_testCoroutine.StepForward();
				cycleCount++;
			}
		}


		[TestMethod]
		public void ItShouldBePossibleToInitalizeControllersManager()
		{
			var ctx = new MockContext(null, null, new MockSessionState(Guid.NewGuid().ToString()));
			var sess = new Mock<ISessionManager>();
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { { "key", "value" } }
			};

			var controllersManager = new ControllersManagerCoroutine();
			controllersManager.IsChildRequest = false;
			controllersManager.ViewData = null;
			controllersManager.Context = ctx;
			controllersManager.SessionManager = sess.Object;
			controllersManager.RouteDefintion = ri;
			controllersManager.InitializeRouteInstance();
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(0, _globalFilter.CountPostExecute);
		}


		[TestMethod]
		public void ItShouldBePossibleToCallANullControllerWith404()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { { "key", "value" } }
			};
			var maxCycles = 2;

			Coroutine.InterceptExternalCalls = false;
			CallCoroutinesWithControllers(ri, maxCycles);


			Assert.AreEqual(1, Exceptions.Count);

			var thrownEx = Exceptions[0] as NodeCsException;
			Assert.IsNotNull(thrownEx);
			Assert.AreEqual(404, thrownEx.HttpCode);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(0, _globalFilter.CountPostExecute);

		}


		[TestMethod]
		public void ItShouldBePossibleToHaveAnErrorWithNonExistingActionWith404()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "nonExisting" } }
			};
			var maxCycles = 2;

			Coroutine.InterceptExternalCalls = true;
			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				var car = src as ICoroutine[];
				if (car != null)
				{
					return new InterceptedStep { TerminateHere = false };
				}
				return Interceptor(src, par);
			};

			CallCoroutinesWithControllers(ri, maxCycles);



			Assert.AreEqual(1, Exceptions.Count);

			var thrownEx = Exceptions[0] as NodeCsException;
			Assert.IsNotNull(thrownEx);
			Assert.AreEqual(404, thrownEx.HttpCode);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(0, _globalFilter.CountPostExecute);

		}


		[TestMethod]
		public void ItShouldBePossibleToCallExistingAction()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestCmAction" } }
			};
			var maxCycles = 10;

			Coroutine.InterceptExternalCalls = false;
			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				var car = src as ICoroutine[];
				if (car != null)
				{
					return new InterceptedStep { TerminateHere = false };
				}
				return Interceptor(src, par);
			};


			CallCoroutinesWithControllers(ri, maxCycles);

			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestCmAction", TestCmController.CalledAction);
			var responseByte = _response.RealStream.GetBuffer();
			Assert.AreNotEqual(0, responseByte.Length);
			var responseText = Encoding.ASCII.GetString(responseByte);
			Assert.AreEqual("TestCmAction", responseText.Substring(0, "TestCmAction".Length));
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}


		[TestMethod]
		public void ItShouldBePossibleToCallAnActionReturningHttpCode()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestHttpCode" } }
			};

			var maxCycles = 10;

			Coroutine.InterceptExternalCalls = false;
			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				var car = src as ICoroutine[];
				if (car != null)
				{
					return new InterceptedStep { TerminateHere = false };
				}
				return Interceptor(src, par);
			};


			CallCoroutinesWithControllers(ri, maxCycles);

			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestHttpCode", TestCmController.CalledAction);
			var responseByte = _response.RealStream.GetBuffer();
			Assert.AreEqual(0, responseByte.Length);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}


		[TestMethod]
		public void ItShouldBePossibleToCallAnActionReturningAViewResponse()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestViewResponse" } }
			};

			MockFilterAtt.CountPostExecute = 0;
			MockFilterAtt.CountPreExecute = 0;


			var maxCycles = 10;

			Coroutine.InterceptExternalCalls = true;
			ViewsManagerCoroutine calledViewsManager = null;
			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				var car = src as ICoroutine[];
				if (car != null && car[0] is ViewsManagerCoroutine)
				{
					calledViewsManager = (ViewsManagerCoroutine)car[0];
					return new InterceptedStep { TerminateHere = true };
				}
				var res = Interceptor(src, par);
				res.TerminateHere = false;
				return res;
			};

			CallCoroutinesWithControllers(ri, maxCycles, () =>
			{
				var rhMock = new Mock<IResponseHandlersFactory>();
				GlobalVars.ResponseHandlers = rhMock.Object;
				rhMock.Setup(a => a.Load(It.IsAny<Type>())).Returns(new MockResponseHandler());
			});


			Assert.IsNotNull(calledViewsManager);
			Assert.IsFalse(calledViewsManager.IsChildRequest);
			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestViewResponse", TestCmController.CalledAction);
			var responseByte = _response.RealStream.GetBuffer();
			Assert.AreEqual(0, responseByte.Length);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
			Assert.AreEqual(1, MockFilterAtt.CountPreExecute);
			Assert.AreEqual(1, MockFilterAtt.CountPostExecute);
		}



		[TestMethod]
		public void ItShouldBePossibleToCallAnActionReturningAViewResponseAsAChild()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestViewResponse" } }
			};

			MockFilterAtt.CountPostExecute = 0;
			MockFilterAtt.CountPreExecute = 0;


			var maxCycles = 10;

			Coroutine.InterceptExternalCalls = true;
			ViewsManagerCoroutine calledViewsManager = null;
			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				var car = src as ICoroutine[];
				if (car != null && car[0] is ViewsManagerCoroutine)
				{
					calledViewsManager = (ViewsManagerCoroutine)car[0];
					return new InterceptedStep { TerminateHere = true };
				}
				var res = Interceptor(src, par);
				res.TerminateHere = false;
				return res;
			};


			CallCoroutinesWithControllers(ri, maxCycles, () =>
			{
				var rhMock = new Mock<IResponseHandlersFactory>();
				GlobalVars.ResponseHandlers = rhMock.Object;
				rhMock.Setup(a => a.Load(It.IsAny<Type>())).Returns(new MockResponseHandler());
			}, true);
			

			Assert.IsNotNull(calledViewsManager);
			Assert.IsTrue(calledViewsManager.IsChildRequest);
			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestViewResponse", TestCmController.CalledAction);
			var responseByte = _response.RealStream.GetBuffer();
			Assert.AreEqual(0, responseByte.Length);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(0, _globalFilter.CountPostExecute);
			Assert.AreEqual(1, MockFilterAtt.CountPreExecute);
			Assert.AreEqual(1, MockFilterAtt.CountPostExecute);
		}
	}
}
