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
using System.Linq;
using GenericHelpers;
using Http.Renderer.Razor.Utils;
using NodeCs.Shared.Attributes;

namespace Http.Renderer.Razor.Helpers
{
	public partial class HtmlHelper<T>
	{
		public Form BeginForm()
		{
			const string verb = "POST";
			const string encType = "application/x-www-form-urlencoded";
			//return BeginForm(action, controller, verb, encType);

			var path = _routingHandler.ResolveFromParams(_context.RouteParams);

			var nodeCsForm = new Form(ViewContext, new Dictionary<string, object>
			{
				{"action",path},
				{"method",verb},
				{"enctype",encType}
			});

			return nodeCsForm;
		}

		public Form BeginForm(string action)
		{
			var controller = (string)_context.RouteParams["controller"];
			const string verb = "POST";
			const string encType = "application/x-www-form-urlencoded";
			return BeginForm(action, controller, verb, encType);
		}

		public Form BeginForm(string action, string controller, string verb, string encType)
		{
			if (controller == null)
			{
				controller = (string)_context.RouteParams["controller"];
			}
			if (action == null)
			{
				action = (string)_context.RouteParams["action"];
			}

			var path = _routingHandler.ResolveFromParams(
					new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
					{
						{"controller",controller},
						{"action",action}
					}
				);

			var nodeCsForm = new Form(ViewContext, new Dictionary<string, object>
			{
				{"action",path},
				{"method",verb},
				{"enctype",encType}
			});

			return nodeCsForm;
		}

		public RawString EditorForModel()
		{
			var result = string.Empty;
			foreach (var property in _classWrapper.Properties)
			{
				var pw = _wrapperDescriptor.GetProperty(property);
				if (!ReflectionUtils.IsSystemType(pw.PropertyType))
				{
					continue;
				}

#pragma warning disable 184
				var scaffold = (ScaffoldColumnAttribute)pw.Attributes.FirstOrDefault(a => (a is ScaffoldColumnAttribute));
				var doScaffold = scaffold == null || scaffold.Scaffold;
#pragma warning restore 184
				if (doScaffold)
				{
					result += EditorFor(pw).ToString();
				}
			}
			return new RawString(result);
		}
	}
}
