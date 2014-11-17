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
using GenericHelpers;
using Http.Renderer.Razor.Integration;
using Http.Routing;
using Http.Shared.Controllers;
using Http.Shared.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeCs.Shared;

namespace HttpRendererRazorTest
{
	namespace Http.Test
	{
		[TestClass]
		public class RazorTemplateGeneratorTest
		{
			[TestMethod]
			public void ItShouldBePossibleToCreateSimpleTemplateWithNoModel()
			{
				ServiceLocator.Locator.Register<IRoutingHandler>(new RoutingService());
				var sourceText = ResourceContentLoader.LoadText("simpleTemplate.cshtml");
				var generator = new RazorTemplateGenerator();
				generator.RegisterTemplate(sourceText, "simpleTemplate");
				generator.CompileTemplates();
				var result = generator.GenerateOutputString(null, "simpleTemplate", null,new ModelStateDictionary());

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
				var result = generator.GenerateOutputString(model, "simpleTemplateString", null, new ModelStateDictionary());

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
				var result = generator.GenerateOutputString(model, "simpleTemplateGeneric", null,new ModelStateDictionary());

				var year = DateTime.UtcNow.Year;
				Assert.IsTrue(result.Contains("Hello World"));
				Assert.IsTrue(result.Contains(year.ToString()));
				Assert.IsTrue(result.Contains(model[0]));
				Assert.IsTrue(result.Contains(model[1]));
			}
		}
	}
}
