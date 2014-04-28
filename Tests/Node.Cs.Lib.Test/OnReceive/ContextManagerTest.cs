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
using Moq;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.Test.Bases;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Test.Mocks;
using Node.Cs.Lib.Contexts;
using System.Web;
using System.Net;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Lib.Test.OnReceive
{
	/// <summary>
	/// Summary description for ContextManagerTest
	/// </summary>
	[TestClass]
	public class ContextManagerTest : BaseCoroutineTest
	{
		[TestMethod]
		public void ItShouldBePossibleToInitializeTheContextManager()
		{
			var listener = new Mock<IListenerContainer>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			var cm = new ContextManager(listener.Object);

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			var result = cm.InitializeContext();
			Assert.AreSame(Step.Current, result);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(Action));
		}

		[TestMethod]
		public void LanguageShouldBeInitialized()
		{
			var originalLanguage = System.Threading.Thread.CurrentThread.CurrentCulture;
			var listener = new Mock<IListenerContainer>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			listener.Setup(a => a.HasUserLanguage).Returns(true);
			listener.Setup(a => a.UserLanguages).Returns(new[] { "es-ES", "fr-FR" });
			GlobalVars.Settings.Listener.Cultures.AvailableCultures.Add("es-ES", new System.Globalization.CultureInfo("es-ES"));

			var cm = new ContextManager(listener.Object);

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			var result = cm.InitializeContext();
			Assert.AreSame(Step.Current, result);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(Action));

			Assert.AreEqual("es-ES", System.Threading.Thread.CurrentThread.CurrentCulture.ToString());

			System.Threading.Thread.CurrentThread.CurrentCulture = originalLanguage;
		}

		[TestMethod]
		public void LanguageShouldBeInitializedWithDefaultIfNotMissing()
		{
			var originalLanguage = System.Threading.Thread.CurrentThread.CurrentCulture;
			var listener = new Mock<IListenerContainer>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			listener.Setup(a => a.HasUserLanguage).Returns(true);
			listener.Setup(a => a.UserLanguages).Returns(new[] { "es-ES", "fr-FR" });
			GlobalVars.Settings.Listener.Cultures.AvailableCultures.Add("en-US", new System.Globalization.CultureInfo("en-US"));
			GlobalVars.Settings.Listener.Cultures.DefaultCultureString = "ar-sa";

			var cm = new ContextManager(listener.Object);

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			var result = cm.InitializeContext();
			Assert.AreSame(Step.Current, result);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(Action));

			Assert.AreEqual("ar-SA", System.Threading.Thread.CurrentThread.CurrentCulture.ToString());

			System.Threading.Thread.CurrentThread.CurrentCulture = originalLanguage;
		}

		[TestMethod]
		public void TwoLettersLanguageShouldBeRecognized()
		{
			var originalLanguage = System.Threading.Thread.CurrentThread.CurrentCulture;
			var listener = new Mock<IListenerContainer>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			listener.Setup(a => a.HasUserLanguage).Returns(true);
			listener.Setup(a => a.UserLanguages).Returns(new[] { "es", "fr-FR" });
			GlobalVars.Settings.Listener.Cultures.AvailableCultures.Add("es-ES", new System.Globalization.CultureInfo("es-ES"));
			GlobalVars.Settings.Listener.Cultures.DefaultCultureString = "en-US";

			var cm = new ContextManager(listener.Object);

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			var result = cm.InitializeContext();
			Assert.AreSame(Step.Current, result);

			Assert.AreEqual(1, Intercepted.Count);
			Assert.IsInstanceOfType(Intercepted[0], typeof(Action));

			Assert.AreEqual("es-ES", System.Threading.Thread.CurrentThread.CurrentCulture.ToString());

			System.Threading.Thread.CurrentThread.CurrentCulture = originalLanguage;
		}

		[TestMethod]
		public void ItShouldBePossibleToInitializeTheResponse()
		{
			var listener = new Mock<IListenerContainer>();
			var routingService = new Mock<IRoutingService>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");

			GlobalVars.RoutingService = routingService.Object;
			var cm = new ContextManagerMock(listener.Object);
			var url = new Uri("http://localhost/test");
			cm.ResultContext = (HttpContextBase)new NodeCsContext(new NodeCsRequest(url));

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			cm.InitializeResponse();

			Assert.AreEqual(0, Intercepted.Count);
			Assert.AreSame(cm.ResultContext, cm.Context);
			Assert.AreSame(url, cm.LocalUrl);
			Assert.AreEqual("/test", cm.LocalPath);
		}

		[TestMethod]
		public void ItShouldBePossibleToPassRouteDefinition()
		{
			var listener = new Mock<IListenerContainer>();
			var routingService = new Mock<IRoutingService>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { { "key", "value" } }
			};
			routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>())).Returns(ri);

			GlobalVars.RoutingService = routingService.Object;
			var cm = new ContextManagerMock(listener.Object);
			var url = new Uri("http://localhost/test");
			cm.ResultContext = (HttpContextBase)new NodeCsContext(new NodeCsRequest(url));

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			cm.InitializeResponse();

			Assert.AreEqual(0, Intercepted.Count);
			Assert.AreSame(cm.ResultContext, cm.Context);
			Assert.AreSame(url, cm.LocalUrl);
			Assert.AreEqual("/test", cm.LocalPath);
			Assert.AreSame(ri, cm.RouteDefintion);
			Assert.IsFalse(cm.IsNotStaticRoute);
		}

		[TestMethod]
		public void ItShouldBePossibleToIdentifyNotStaticRoute()
		{
			var listener = new Mock<IListenerContainer>();
			var routingService = new Mock<IRoutingService>();
			GlobalVars.Settings = NodeCsSettings.Defaults("C:\\");
			var ri = new RouteInstance(false)
			{
				Parameters = new Dictionary<string, object> { 
					{ "key", "value" },
					{ "action", "action" },
					{ "controller", "controller" }
				}
			};
			routingService.Setup(a => a.Resolve(It.IsAny<string>(), It.IsAny<HttpContextBase>())).Returns(ri);

			GlobalVars.RoutingService = routingService.Object;
			var cm = new ContextManagerMock(listener.Object);
			var url = new Uri("http://localhost/test");
			cm.ResultContext = (HttpContextBase)new NodeCsContext(new NodeCsRequest(url));

			InitializeListener();

			// ReSharper disable once ConvertClosureToMethodGroup
			Coroutine.Interceptor = (src, par) =>
			{
				return Interceptor(src, par);
			};

			cm.InitializeResponse();

			Assert.AreEqual(0, Intercepted.Count);
			Assert.AreSame(cm.ResultContext, cm.Context);
			Assert.AreSame(url, cm.LocalUrl);
			Assert.AreEqual("/test", cm.LocalPath);
			Assert.AreSame(ri, cm.RouteDefintion);
			Assert.IsTrue(cm.IsNotStaticRoute);
		}
	}
}
