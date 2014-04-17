using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace Node.Cs.Lib.Test
{
	[TestClass]
	public class OnHttpListenerReceivedTest : OnHttpListenerReceivedTestBase
	{

		[TestMethod]
		public void RequiringNonExistingPageShouldThrowException()
		{
			InitializeNodeCs("http://localhost/test", "test.cshtml");

			_extensionHandler.Setup(a => a.Extensions).Returns(new ReadOnlyCollection<string>(new List<string>()));

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
								 {
									 Parameters = new Dictionary<string, object>()
								 });

			var ctx = new MockContext(_request, null, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);
			try
			{
				res.Run().ToList();
			}
			catch (NodeCsException ex)
			{
				Assert.AreEqual(404, ex.HttpCode);
				Assert.AreEqual("Resource '/test' not found.", ex.Message);
				return;
			}
			Assert.Fail("No exception thrown");
		}

		[TestMethod]
		public void IfNoHandlerIsPresent500ShouldBeThrown()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test", "testContent");

			_extensionHandler.Setup(a => a.Extensions).Returns(new ReadOnlyCollection<string>(new List<string>()));

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});

			var ctx = new MockContext(_request, null, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);
			try
			{
				res.Run().ToList();
			}
			catch (NodeCsException ex)
			{
				Assert.AreEqual(500, ex.HttpCode);
				Assert.AreEqual("No handler found for resource '/test'.", ex.Message);
				return;
			}
			Assert.Fail("No exception thrown");
		}

		[TestMethod]
		public void IfHandlerIsPresentCallHandlerInstanceShouldBeReturned()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test.html", "testContent");

			var coThread = new CoroutineTestThread();

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new StaticHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});

			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			Assert.AreEqual(1, res.CallHandlerInstanceCalls);
		}

		[TestMethod]
		public void SessionBasedHandlersShouldStoreSessions()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test.html", "testContent");

			var coThread = new CoroutineTestThread();

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);


			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new MockHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});

			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			Assert.AreEqual(1, res.CallHandlerInstanceCalls);
			_sessionStorage.Verify(a => a.CreateSession(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));
			_sessionStorage.Verify(a => a.StoreSession(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
		}


		[TestMethod]
		public void PrePostGlobalFiltersShouldBeHandled()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test.html", "testContent");

			var coThread = new CoroutineTestThread();

			var filter = new Mock<MockFilter>();
			GlobalVars.GlobalFilters.Add(filter.Object);

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new MockHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});


			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			filter.Verify(a => a.OnPreExecute(It.IsAny<HttpContextBase>()), Times.Once);
			filter.Verify(a => a.OnPostExecute(It.IsAny<HttpContextBase>(), It.IsAny<IResponse>()), Times.Once);
		}

		[TestMethod]
		public void UserLanguagesShouldBeHonouredIfPresentOnSettings()
		{
			var prev = System.Threading.Thread.CurrentThread.CurrentCulture;

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

			InitializeNodeCs("http://localhost/test", "test.html");


			GlobalVars.Settings.Listener.Cultures.AvailableCultureStrings = "en-US,fr-FR";

			_pathProvider.Files.Add("test.html", "testContent");
			_container.Setup(a => a.UserLanguages).Returns(new[] {  "fr-FR" });
			_container.Setup(a => a.HasUserLanguage).Returns(true);

			var coThread = new CoroutineTestThread();

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new StaticHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});

			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			Assert.AreEqual("fr", System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
			System.Threading.Thread.CurrentThread.CurrentCulture = prev;
		}


		[TestMethod]
		public void UserLanguagesShouldFallbackToDefault()
		{
			var prev = System.Threading.Thread.CurrentThread.CurrentCulture;

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

			InitializeNodeCs("http://localhost/test", "test.html");


			GlobalVars.Settings.Listener.Cultures.AvailableCultureStrings = "en-US,fr-FR";

			_pathProvider.Files.Add("test.html", "testContent");
			_container.Setup(a => a.UserLanguages).Returns(new[] { "es-ES" });
			_container.Setup(a => a.HasUserLanguage).Returns(true);

			var coThread = new CoroutineTestThread();

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new StaticHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});

			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			Assert.AreEqual("en", System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
			System.Threading.Thread.CurrentThread.CurrentCulture = prev;
		}

		[TestMethod]
		public void ShouldBeSeekdThePageAppendingIndex()
		{
			InitializeNodeCs("http://localhost/test", "test.html");

			_pathProvider.Files.Add("test/index", "testContent");
			_pathProvider.Dirs.Add("test", string.Empty);

			var coThread = new CoroutineTestThread();

			var filter = new Mock<MockFilter>();
			GlobalVars.GlobalFilters.Add(filter.Object);

			_nodeCsServer.Setup(a => a.NextCoroutine).Returns(coThread);

			_extensionHandler.Setup(a => a.Extensions)
				.Returns(new ReadOnlyCollection<string>(new List<string> { ".html" }));

			_extensionHandler.Setup(a => a.CreateInstance(It.IsAny<HttpContextBase>(), It.IsAny<PageDescriptor>()))
				.Returns(new MockHandler());

			_routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
				.Returns(new RouteInstance()
				{
					Parameters = new Dictionary<string, object>()
				});


			var ctx = new MockContext(_request, _response, new MockSessionState(Guid.NewGuid().ToString()));
			var res = new OnHttpListenerReceivedCoroutineForTest(ctx);

			res.Initialize(null,null,_nodeCsServer.Object, _container.Object);

			foreach (var item in res.Run())
			{
				Console.Write(".");
			}
			filter.Verify(a => a.OnPreExecute(It.IsAny<HttpContextBase>()), Times.Once);
			filter.Verify(a => a.OnPostExecute(It.IsAny<HttpContextBase>(), It.IsAny<IResponse>()), Times.Once);
		}
	}
}
