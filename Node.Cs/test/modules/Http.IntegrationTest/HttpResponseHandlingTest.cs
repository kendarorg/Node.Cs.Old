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


using CoroutinesLib.TestHelpers;
using HtmlAgilityPack;
using Http.Contexts;
using Http.PathProvider.StaticContent;
using Http.Routing;
using Http.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Caching;
using Node.Cs.TestHelpers;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Http.IntegrationTest
{
	[TestClass]
	public class HttpResponseHandlingTest : BaseResponseHandlingTest
	{
		[TestMethod]
		public void ItShouldBePossibleToExecuteArequest()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
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

			//request.
			http.ExecuteRequest(context);
			runner.RunCycleFor(200);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var result = Encoding.UTF8.GetString(bytes);

			Assert.IsTrue(outputStream.WrittenBytes > 0);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Length > 0);
			Assert.IsTrue(result.IndexOf("Exception", StringComparison.Ordinal) < 0, result);
			Assert.AreEqual(1, outputStream.ClosesCall);
		}


		[TestMethod]
		public void ItShouldBePossibleToExecuteArequestWithCache()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var cachingModule = new CachingModule();
			cachingModule.Initialize();
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
			pathProvider.SetCachingEngine(cachingModule.GetParameter<ICacheEngine>(HttpParameters.CacheInstance));
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

			//request.
			http.ExecuteRequest(context);
			runner.RunCycleFor(200);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var result = Encoding.UTF8.GetString(bytes);

			Assert.IsNotNull(result);
			Assert.IsTrue(outputStream.WrittenBytes > 0);
			Assert.IsTrue(result.Length > 0);
			Assert.IsTrue(result.IndexOf("Exception", StringComparison.Ordinal) < 0, result);
			Assert.AreEqual(1, outputStream.ClosesCall);
		}

		[TestMethod]
		public void ItShouldBePossibleToSimulateMulitpleRequests()
		{
			ItShouldBePossibleToSimulateRequest();
			ItShouldBePossibleToSimulateRequest();
			ItShouldBePossibleToSimulateRequest();
			ItShouldBePossibleToSimulateRequest();
		}

		[TestMethod]
		public void ItShouldBePossibleToSimulateRequest()
		{
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var runner = new RunnerForTest();
			NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);
			var rootDir = InferWebRootDir(assemblyLocation);
			var pathProvider = new StaticContentPathProvider(rootDir);
			var http = new HttpModule();
			http.SetParameter(HttpParameters.HttpPort, 8881);
			http.SetParameter(HttpParameters.HttpVirtualDir, "nodecs");
			http.SetParameter(HttpParameters.HttpHost, "localhost");


			var routingService = new RoutingService();
			http.RegisterRouting(routingService);

			http.Initialize();

			http.RegisterPathProvider(pathProvider);

			const string uri = "http://localhost:8881/nodecs";


			var bytes = RunRequest(uri + "/numbers.htm", http, runner);
			var result = Encoding.UTF8.GetString(bytes);
			var html = new HtmlDocument();
			html.LoadHtml(result);

			var nodes = html.DocumentNode.SelectNodes("//img/@src");
			var contexts = new List<SimpleHttpContext>();
			if (nodes != null)
			{
				foreach (HtmlNode node in nodes)
				{
					var src = node.Attributes["src"].Value;
					if (!src.StartsWith("http")) src = uri + "/" + src.Trim('/');
					contexts.Add((SimpleHttpContext)PrepareRequest(src));
					http.ExecuteRequest(contexts.Last());
				}
			}
			runner.RunCycleFor(500);
			for (int index = 0; index < contexts.Count; index++)
			{
				var ctx = contexts[index];
				VerifyContext(ctx);
			}
		}



	}
}
