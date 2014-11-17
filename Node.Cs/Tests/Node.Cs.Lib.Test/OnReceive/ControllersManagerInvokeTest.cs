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
	/// Summary description for ControllersManagerInvokeTest
	/// </summary>
	[TestClass]
	public class ControllersManagerInvokeTest : BaseCoroutineTest
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


		private void CallCoroutinesWithControllers(RouteInstance ri, int maxCycles, Action actionBeforeRun = null, bool childRequest = false, HttpRequestWrapper wrapper = null)
		{

			Exceptions.Clear();
			var tw = new StringWriter();
			_response = new MockResponse(new HttpResponse(tw));
			var request = wrapper ?? new HttpRequestWrapper(new HttpRequest("test", "http://local/test", ""));
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
		public void ItShouldBePossibleToCallActionWithStringParam()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponse" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par1=par1test"));
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

			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponse", TestCmController.CalledAction);

			Assert.AreEqual(1, TestCmController.Data.Count);
			Assert.AreEqual("par1test", TestCmController.Data["par1"].ToString());
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}

		[TestMethod]
		public void ItShouldBePossibleToCallActionWithIntParam()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntString" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par2=33&par1=par1test"));
			var maxCycles = 10;
			Coroutine.InterceptExternalCalls = false;


			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponseIntString", TestCmController.CalledAction);

			Assert.AreEqual(2, TestCmController.Data.Count);
			Assert.AreEqual("par1test", TestCmController.Data["par1"].ToString());
			Assert.AreEqual("33", TestCmController.Data["par2"].ToString());
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}

		[TestMethod]
		[Ignore]
		public void ItShouldBePossibleToCallActionWithComplexParamInQueryString()
		{
			//TODO: Complex parameters in query string
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntString" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par1.length=33&par1.text=par1test"));
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

			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponseIntString", TestCmController.CalledAction);

			Assert.AreEqual(2, TestCmController.Data.Count);
			Assert.AreEqual("par1test", TestCmController.Data["par1"].ToString());
			Assert.AreEqual("33", TestCmController.Data["par2"].ToString());
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}

		[TestMethod]
		public void ItShouldBePossibleToCallActionWithNullbaleIntParam()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntNullString" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par1=par1test"));
			var maxCycles = 10;
			Coroutine.InterceptExternalCalls = false;


			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponseIntNullString", TestCmController.CalledAction);

			Assert.AreEqual(2, TestCmController.Data.Count);
			Assert.AreEqual("par1test", TestCmController.Data["par1"].ToString());
			Assert.IsNull(TestCmController.Data["par2"]);
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}


		[TestMethod]
		public void ItShouldBePossibleToCallActionWithOptionalIntParam()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntOptional" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", ""));
			var maxCycles = 10;
			Coroutine.InterceptExternalCalls = false;


			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponseIntOptional", TestCmController.CalledAction);

			Assert.AreEqual(1, TestCmController.Data.Count);
			Assert.AreEqual("4", TestCmController.Data["par1"].ToString());
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}



		[TestMethod]
		public void ItShouldBePossibleToCallActionWithOptionalIntParamWithValue()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntNullString" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par1=22"));
			var maxCycles = 10;
			Coroutine.InterceptExternalCalls = false;


			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);


			Assert.AreEqual(0, Exceptions.Count);
			Assert.AreEqual("TestDataResponseIntNullString", TestCmController.CalledAction);

			Assert.AreEqual(2, TestCmController.Data.Count);
			Assert.AreEqual("22", TestCmController.Data["par1"].ToString());
			Assert.AreEqual(0, _globalFilter.CountPreExecute);
			Assert.AreEqual(1, _globalFilter.CountPostExecute);
		}

		[TestMethod]
		public void ItShouldNotBePossibleToCallActionWithIntParamNotSpecified()
		{
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
				{ "controller", "TestCm" } ,
				{ "action", "TestDataResponseIntString" } }
			};
			var requestWrapper = new HttpRequestWrapper(new HttpRequest("test", "http://local/test", "par1=par1test"));
			var maxCycles = 10;
			Coroutine.InterceptExternalCalls = false;


			CallCoroutinesWithControllers(ri, maxCycles, wrapper: requestWrapper);

			Assert.AreEqual(1, Exceptions.Count);
			var thrownEx = Exceptions[0] as NodeCsException;
			Assert.IsNotNull(thrownEx);
			Assert.AreEqual(404, thrownEx.HttpCode);
		}
	}
}
