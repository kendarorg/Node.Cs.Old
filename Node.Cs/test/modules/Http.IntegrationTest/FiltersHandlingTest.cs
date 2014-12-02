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


using System.IO;
using System.Reflection;
using System.Text;
using CoroutinesLib.TestHelpers;
using Http.PathProvider.StaticContent;
using Http.Renderer.Markdown;
using Http.Renderer.Razor;
using Http.Routing;
using Http.Shared;
using Http.Shared.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node.Cs.TestHelpers;
using NodeCs.Shared;
using System;

namespace Http.IntegrationTest
{
	[TestClass]
	public class FiltersHandlingTest : BaseResponseHandlingTest
	{
		[TestMethod]
		public void StaticRenderer_WhenExecutingArequest_ShouldApplyFilters()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
			var filterHandler = new FilterHandler();

			var globalFilter = new Mock<IFilter>();
			globalFilter.Setup(a => a.OnPreExecute(It.IsAny<IHttpContext>())).Returns(true);
			filterHandler.AddFilter(globalFilter.Object);
			ServiceLocator.Locator.Register<IFilterHandler>(filterHandler);

			var http = new HttpModule();
			http.SetParameter(HttpParameters.HttpPort, 8881);
			http.SetParameter(HttpParameters.HttpVirtualDir, "nodecs");
			http.SetParameter(HttpParameters.HttpHost, "localhost");


			var routingService = new RoutingService();
			http.RegisterRouting(routingService);

			http.Initialize();

			http.RegisterPathProvider(pathProvider);

			const string uri = "http://localhost:8881/nodecs";

			var context = CreateRequest(uri);
			var outputStream = (MockStream)context.Response.OutputStream;
			outputStream.Initialize();


            globalFilter.Setup(a => a.OnPostExecute(It.IsAny<IHttpContext>()))
                .Callback(() =>
                {
                    Console.WriteLine("AAAA");
                });

			//request.
			http.ExecuteRequest(context);
			runner.RunCycleFor(200);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var result = Encoding.UTF8.GetString(bytes);

			Assert.IsTrue(outputStream.WrittenBytes > 0);
			Assert.IsNotNull(result);

			globalFilter.Verify(a => a.OnPreExecute(It.IsAny<IHttpContext>()), Times.Once);
			globalFilter.Verify(a => a.OnPostExecute(It.IsAny<IHttpContext>()), Times.Once);
		}

		[TestMethod]
		public void RenderizableRenderer_WhenExecutingArequest_ShouldApplyFilters()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
			var filterHandler = new FilterHandler();

			var globalFilter = new Mock<IFilter>();
			globalFilter.Setup(a => a.OnPreExecute(It.IsAny<IHttpContext>())).Returns(true);
			filterHandler.AddFilter(globalFilter.Object);
			ServiceLocator.Locator.Register<IFilterHandler>(filterHandler);

			var http = new HttpModule();
			http.SetParameter(HttpParameters.HttpPort, 8881);
			http.SetParameter(HttpParameters.HttpVirtualDir, "nodecs");
			http.SetParameter(HttpParameters.HttpHost, "localhost");
			http.RegisterRenderer(new MarkdownRenderer());


			var routingService = new RoutingService();
			http.RegisterRouting(routingService);

			http.Initialize();

			http.RegisterPathProvider(pathProvider);

			const string uri = "http://localhost:8881/nodecs/index.md";

			var context = CreateRequest(uri);
			var outputStream = (MockStream)context.Response.OutputStream;
			outputStream.Initialize();

			//request.
			http.ExecuteRequest(context);
			runner.RunCycleFor(700);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var result = Encoding.UTF8.GetString(bytes);

			Assert.IsTrue(outputStream.WrittenBytes > 0);
			Assert.IsNotNull(result);

			globalFilter.Verify(a => a.OnPreExecute(It.IsAny<IHttpContext>()), Times.Once);
			globalFilter.Verify(a => a.OnPostExecute(It.IsAny<IHttpContext>()), Times.Once);
		}


		[TestMethod]
		public void RenderizableRenderer_WhenExecutingArequestWithException_ShouldApplyFilters()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
			var filterHandler = new FilterHandler();

			var globalFilter = new Mock<IFilter>();
			globalFilter.Setup(a => a.OnPreExecute(It.IsAny<IHttpContext>())).Returns(true);
			filterHandler.AddFilter(globalFilter.Object);
			ServiceLocator.Locator.Register<IFilterHandler>(filterHandler);

			var http = new HttpModule();
			http.SetParameter(HttpParameters.HttpPort, 8881);
			http.SetParameter(HttpParameters.HttpVirtualDir, "nodecs");
			http.SetParameter(HttpParameters.HttpHost, "localhost");
			http.RegisterRenderer(new RazorRenderer());


			var routingService = new RoutingService();
			http.RegisterRouting(routingService);

			http.Initialize();

			http.RegisterPathProvider(pathProvider);

			const string uri = "http://localhost:8881/nodecs/exploding.cshtml";

			var context = CreateRequest(uri);
			var outputStream = (MockStream)context.Response.OutputStream;
			outputStream.Initialize();

			//request.
			http.ExecuteRequest(context);
			runner.RunCycleFor(2000);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var result = Encoding.UTF8.GetString(bytes);

			Assert.IsTrue(outputStream.WrittenBytes > 0);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("format"), result);

			globalFilter.Verify(a => a.OnPreExecute(It.IsAny<IHttpContext>()), Times.Once);
			globalFilter.Verify(a => a.OnPostExecute(It.IsAny<IHttpContext>()), Times.Once);
		}
		

	}
}