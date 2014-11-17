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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Razor;
using Microsoft.CSharp;

namespace Http.Renderer.Razor.Integration
{
	public class Compiler
	{
		private static string TypeToString(Type type)
		{
			var typeName = type.Name;
			var thinghy = typeName.IndexOf('`');
			if (thinghy > 0)
			{
				typeName = typeName.Substring(0, thinghy);
			}
			var res = type.Namespace + "." + typeName;
			if (type.IsGenericType)
			{
				res += "<";
				var args = type.GetGenericArguments();
				for (int i = 0; i < args.Length; i++)
				{
					if (i > 0) res += ", ";
					res += TypeToString(args[i]);
				}
				res += ">";
			}
			return res;
		}

		private static GeneratorResults GenerateCode(RazorTemplateEntry entry)
		{
			var host = new RazorEngineHost(new CSharpRazorCodeLanguage());

			host.DefaultBaseClass = string.Format("Http.Renderer.Razor.Integration.RazorTemplateBase<{0}>", TypeToString(entry.ModelType));
			host.DefaultNamespace = "Http.Renderer.Razor.Integration";
			host.DefaultClassName = entry.TemplateName + "Template";
			host.NamespaceImports.Add("System");
			GeneratorResults razorResult = null;
			using (TextReader reader = new StringReader(entry.TemplateString))
			{
				razorResult = new RazorTemplateEngine(host).GenerateCode(reader);
			}
			return razorResult;
		}

		private static CompilerParameters BuildCompilerParameters()
		{
			var @params = new CompilerParameters();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.ManifestModule.Name != "<In Memory Module>")
					@params.ReferencedAssemblies.Add(assembly.Location);
			}
			@params.GenerateInMemory = true;
			@params.IncludeDebugInformation = false;
			@params.GenerateExecutable = false;
			@params.CompilerOptions = "/target:library /optimize";
			return @params;
		}

		public static Assembly Compile(IEnumerable<RazorTemplateEntry> entries)
		{
			var builder = new StringBuilder();
			var codeProvider = new CSharpCodeProvider();
			using (var writer = new StringWriter(builder))
			{
				var entriesArray = entries.ToArray();
				for (int index = 0; index < entriesArray.Length; index++)
				{
					var razorTemplateEntry = entriesArray[index];
					if (razorTemplateEntry.TemplateType == null)
					{
						var generatorResults = GenerateCode(razorTemplateEntry);
						codeProvider.GenerateCodeFromCompileUnit(generatorResults.GeneratedCode, writer, new CodeGeneratorOptions());
					}
				}
			}

			var result = codeProvider.CompileAssemblyFromSource(BuildCompilerParameters(), new[] { builder.ToString() });
			if (result.Errors != null && result.Errors.Count > 0)
				throw new TemplateCompileException(result.Errors, builder.ToString());

			return result.CompiledAssembly;
		}
	}
}