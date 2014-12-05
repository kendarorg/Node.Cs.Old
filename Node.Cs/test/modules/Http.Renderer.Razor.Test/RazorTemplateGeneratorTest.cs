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
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoroutinesLib;
using CoroutinesLib.Shared;
using CoroutinesLib.TestHelpers;
using GenericHelpers;
using Http;
using Http.Contexts;
using Http.Renderer.Razor.Integration;
using Http.Routing;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.TestHelpers;
using NodeCs.Shared;

namespace HttpRendererRazorTest
{
	namespace Http.Test
	{
		[TestClass]
		public class RazorTemplateGeneratorTest
		{
			private string GetResult(IEnumerable<ICoroutineResult> res)
			{
				var ms = new MemoryStream();
				foreach (var item in res)
				{
					var bytes = item.Result as byte[];
					if (bytes != null)
					{
						ms.Write(bytes,0,bytes.Length);
					}
				}
				return Encoding.UTF8.GetString(ms.ToArray());
			}
			[TestMethod]
			public void ItShouldBePossibleToCreateSimpleTemplateWithNoModel()
			{
				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var sourceText = ResourceContentLoader.LoadText("simpleTemplate.cshtml");
				var generator = new RazorTemplateGenerator();
				generator.RegisterTemplate(sourceText, "simpleTemplate");
				generator.CompileTemplates();
				var result = GetResult(generator.GenerateOutputString(null, "simpleTemplate", null, new ModelStateDictionary(), new ExpandoObject()));

				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Hello World"));
				Assert.IsTrue(result.Contains(year.ToString()));
			}

			[TestMethod]
			public void ItShouldBePossibleToCreateSimpleTemplateWithStringModel()
			{
				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var sourceText = ResourceContentLoader.LoadText("simpleTemplateString.cshtml");
				var generator = new RazorTemplateGenerator();
				generator.RegisterTemplate(sourceText, "simpleTemplateString");
				generator.CompileTemplates();
				var model = "This is the model";
				var result = GetResult(generator.GenerateOutputString(model, "simpleTemplateString", null, new ModelStateDictionary(), new ExpandoObject()));

				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Hello World"));
				Assert.IsTrue(result.Contains(year.ToString()));
				Assert.IsTrue(result.Contains(model));
			}

			[TestMethod]
			public void ItShouldBePossibleToCreateSimpleTemplateWithGenericModel()
			{
				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var sourceText = ResourceContentLoader.LoadText("simpleTemplateGeneric.cshtml");
				var generator = new RazorTemplateGenerator();
				generator.RegisterTemplate(sourceText, "simpleTemplateGeneric");
				generator.CompileTemplates();
				var model = new List<string>
				{
					"First",
					"Second"
				};
				var result = GetResult(generator.GenerateOutputString(model, "simpleTemplateGeneric", null, new ModelStateDictionary(), new ExpandoObject()));

				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Hello World"));
				Assert.IsTrue(result.Contains(year.ToString()));
				Assert.IsTrue(result.Contains(model[0]));
				Assert.IsTrue(result.Contains(model[1]));
			}


			[TestMethod]
			public void ItShouldBePossibleToCreateTemplateWithSection()
			{
				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var sourceText = ResourceContentLoader.LoadText("section.cshtml");
				var generator = new RazorTemplateGenerator();
				generator.RegisterTemplate(sourceText, "simpleTemplateGeneric");
				generator.CompileTemplates();

				var result = GetResult(generator.GenerateOutputString(null, "simpleTemplateGeneric", null, new ModelStateDictionary(), new ExpandoObject()));

				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Hello World"));
				Assert.IsTrue(result.Contains("C:\\test"));
			}


			[TestMethod]
			public void ItShouldBePossibleToCreateTemplateWithRenderPage()
			{
				RunnerFactory.Initialize();
				var runner = RunnerFactory.Create();

				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var httpModule = ServiceLocator.Locator.Resolve<HttpModule>();
				var resPathProvider = new ResourcesPathProvider(Assembly.GetExecutingAssembly());
				resPathProvider.RegisterPath("renderPage.cshtml", "renderPage.cshtml");
				resPathProvider.RegisterPath("renderPageSub.cshtml", "renderPageSub.cshtml");

				httpModule.RegisterPathProvider(resPathProvider);

				//var assemblyLocation = Assembly.GetExecutingAssembly().Location;
				//NodeMainInitializer.InitializeFakeMain(assemblyLocation, runner);

				runner.Start();
				var generator = new RazorTemplateGenerator();
				var sourceText = ResourceContentLoader.LoadText("renderPage.cshtml");
				generator.RegisterTemplate(sourceText, "renderPage");
				sourceText = ResourceContentLoader.LoadText("renderPageSub.cshtml");
				generator.RegisterTemplate(sourceText, "renderPageSub");

				generator.CompileTemplates();

				string result = string.Empty;
				Task.Run(() =>
				{
					var ctx = CreateRequest("http://127.0.0.1/renderPage.cshtml");
					result = GetResult(generator.GenerateOutputString(null, "renderPage", ctx, new ModelStateDictionary(), new ExpandoObject()));
				});
				Thread.Sleep(1000);

				runner.Stop();
				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Mainpage"));
				Assert.IsTrue(result.Contains("Subpage"));
			}

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
		}
	}
}
