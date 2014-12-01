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
using Http.Contexts;
using Http.Shared.Routing;
using HttpMvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node.Cs.TestHelpers;

namespace Http.Routing.Test
{
	public class MockController : ControllerBase
	{

	}

	// ReSharper disable PossibleNullReferenceException
	[TestClass]
	public class RoutingServiceTest
	{
		private static SimpleHttpContext CreateRequest(string uri)
		{
			var context = new SimpleHttpContext();
			var request = new SimpleHttpRequest();
			request.SetUrl(new Uri(uri));
			var response = new SimpleHttpResponse();
			var outputStream = new MockStream();
			response.SetOutputStream(outputStream);
			context.SetRequest(request);
			context.SetResponse(response);
			return context;
		}
		[TestMethod]
		public void ItShouldBePossibleToHandleDirectUrlParameters()
		{
			IRoutingHandler rs = new RoutingService();
			rs.LoadControllers(new[] { typeof(MockController) });
			rs.MapRoute("","~/{controller}/{method}/{id}", new { Controller = "Mock", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = CreateRequest("http://127.0.0.1/Mock/Index");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(3, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Mock", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
		}


		[TestMethod]
		public void ItShouldBePossibleToHandleUrlWithParameterSetValueOnly()
		{
			IRoutingHandler rs = new RoutingService();
			rs.LoadControllers(new[] { typeof(MockController) });
			rs.MapRoute("", "~/{controller}/Fuffa/{id}", new { Controller = "Mock", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = CreateRequest("http://127.0.0.1/Mock/Fuffa");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Mock", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
		}

		[TestMethod]
		public void ItShouldBePossibleToDistinguishTwoDifferentRequest()
		{
			IRoutingHandler rs = new RoutingService();
			rs.LoadControllers(new[] { typeof(MockController) });
			rs.MapRoute("", "~/{controller}/Index/{id}", new { Controller = "Mock", Action = "MethodIndex", Id = RoutingParameter.Optional });
			rs.MapRoute("", "~/{controller}/Test/{id}", new { Controller = "Mock", Action = "MethodTest", Id = RoutingParameter.Optional });

			var ctx = CreateRequest("http://127.0.0.1/Mock/Index");
			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Mock", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("MethodIndex", result.Parameters["action"]);

			ctx = CreateRequest("http://127.0.0.1/Mock/Test");
			result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Mock", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("MethodTest", result.Parameters["action"]);
		}


		[TestMethod]
		public void ItShouldBePossibleToSetOptionalParameters()
		{
			IRoutingHandler rs = new RoutingService();
			rs.LoadControllers(new []{typeof(MockController)});
			rs.MapRoute("", "~/{controller}/Fuffa/{id}", new { Controller = "Mock", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = CreateRequest("http://127.0.0.1/Mock/Fuffa/200");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(3, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Mock", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
			Assert.IsTrue(result.Parameters.ContainsKey("id"));
			Assert.AreEqual("200", result.Parameters["id"]);
		}

		[TestMethod]
		[Ignore]
		public void ItShouldBePossibleToHandleStarParametersPath()
		{

		}

		[TestMethod]
		[Ignore]
		public void ItShouldBePossibleToOverrideRoutes()
		{

		}

		[TestMethod]
		[Ignore]
		public void ItShouldBePossibleToIgnoreRoutes()
		{

		}
	}
	// ReSharper restore PossibleNullReferenceException
}
