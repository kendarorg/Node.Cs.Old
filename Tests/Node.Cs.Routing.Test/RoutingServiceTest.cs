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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Routing.Test
{
	// ReSharper disable PossibleNullReferenceException
	[TestClass]
	public class RoutingServiceTest
	{
		[TestMethod]
		public void ItShouldBePossibleToHandleDirectUrlParameters()
		{
			var cf = new Mock<IControllersFactory>();
			cf.Setup(c => c.Create(It.IsAny<string>())).Returns(new MockController());
			IRoutingService rs = new RoutingService();
			rs.AddRoute("~/{controller}/{method}/{id}", new { Controller = "Home", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = Utils.MockContext("http://127.0.0.1/Home/Index");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(3, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Home", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
		}


		[TestMethod]
		public void ItShouldBePossibleToHandleUrlWithParameterSetValueOnly()
		{
			var cf = new Mock<IControllersFactory>();
			cf.Setup(c => c.Create(It.IsAny<string>())).Returns(new MockController());
			IRoutingService rs = new RoutingService();
			rs.AddRoute("~/{controller}/Fuffa/{id}", new { Controller = "Home", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = Utils.MockContext("http://127.0.0.1/Home/Fuffa");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Home", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
		}

		[TestMethod]
		public void ItShouldBePossibleToDistinguishTwoDifferentRequest()
		{
			var cf = new Mock<IControllersFactory>();
			cf.Setup(c => c.Create(It.IsAny<string>())).Returns(new MockController());
			IRoutingService rs = new RoutingService();
			rs.AddRoute("~/{controller}/Index/{id}", new { Controller = "Home", Action = "MethodIndex", Id = RoutingParameter.Optional });
			rs.AddRoute("~/{controller}/Test/{id}", new { Controller = "Home", Action = "MethodTest", Id = RoutingParameter.Optional });

			var ctx = Utils.MockContext("http://127.0.0.1/Home/Index");
			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Home", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("MethodIndex", result.Parameters["action"]);

			ctx = Utils.MockContext("http://127.0.0.1/Home/Test");
			result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(2, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Home", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("MethodTest", result.Parameters["action"]);
		}


		[TestMethod]
		public void ItShouldBePossibleToSetOptionalParameters()
		{
			var cf = new Mock<IControllersFactory>();
			cf.Setup(c => c.Create(It.IsAny<string>())).Returns(new MockController());
			IRoutingService rs = new RoutingService();
			rs.AddRoute("~/{controller}/Fuffa/{id}", new { Controller = "Home", Action = "Index", Id = RoutingParameter.Optional });
			var ctx = Utils.MockContext("http://127.0.0.1/Home/Fuffa/200");

			var result = rs.Resolve(ctx.Request.Url.LocalPath, ctx);
			Assert.IsNotNull(result);

			Assert.AreEqual(3, result.Parameters.Count);
			Assert.IsTrue(result.Parameters.ContainsKey("controller"));
			Assert.AreEqual("Home", result.Parameters["controller"]);
			Assert.IsTrue(result.Parameters.ContainsKey("action"));
			Assert.AreEqual("Index", result.Parameters["action"]);
			Assert.IsTrue(result.Parameters.ContainsKey("id"));
			Assert.AreEqual("200", result.Parameters["id"]);
		}
	}
	// ReSharper restore PossibleNullReferenceException
}
