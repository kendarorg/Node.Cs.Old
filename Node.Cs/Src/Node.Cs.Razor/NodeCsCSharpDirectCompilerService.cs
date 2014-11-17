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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Web.Razor.Parser;
using ExpressionBuilder;
using Microsoft.CSharp;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Loggers;
using RazorEngine.Compilation;
using RazorEngine.Compilation.CSharp;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using Node.Cs.Lib;


namespace Node.Cs.Razor
{
	public class NodeCsCSharpDirectCompilerService : DirectCompilerServiceBase
	{

		private readonly CSharpCodeProvider _codeDomProvider;

		#region Constructor

		/// <summary>
		/// Initialises a new instance of <see cref="CSharpDirectCompilerService"/>.
		/// </summary>
		/// <param name="strictMode">Specifies whether the strict mode parsing is enabled.</param>
		/// <param name="markupParserFactory">The markup parser factory to use.</param>
		/// <param name="codeProvider"></param>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed"),
		 SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
			 Justification = "Disposed in base class: DirectCompilerServiceBase")]
		public NodeCsCSharpDirectCompilerService(bool strictMode = true,
			Func<ParserBase> markupParserFactory = null, CSharpCodeProvider codeProvider = null)
			: base(
				new CSharpRazorCodeLanguage(strictMode),
				codeProvider ?? new CSharpCodeProvider(),
				markupParserFactory)
		{
			_extraLocations = new List<string> { GlobalVars.ApplicationLocation };
			_codeDomProvider = codeProvider;
		}
		private readonly List<string> _extraLocations;
		#endregion

		#region Methods
		/// <summary>
		/// Returns a set of assemblies that must be referenced by the compiled template.
		/// </summary>
		/// <returns>The set of assemblies.</returns>
		public override IEnumerable<string> IncludeAssemblies()
		{
			var all = new List<String>(_extraLocations);
			all.Add(typeof(Function).Assembly.Location);
			all.Add(typeof(IResourceHandler).Assembly.Location);
			all.Add(typeof(Binder).Assembly.Location);
			all.Add(GetType().Assembly.Location);
			return all.ToArray();
		}


		[Pure, SecurityCritical]
		public override Tuple<Type, Assembly> CompileType(TypeContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			RazorHandler.CleanModelType(context);

			ParseModelFromContext(context);
			//context.ModelType = typeof(ExpandoObject);
			var result = Compile(context);
			var compileResult = result.Item1;

			if (compileResult.Errors != null && compileResult.Errors.HasErrors)
			{
				for (int i = 0; i < compileResult.Errors.Count; i++)
				{
					var item = compileResult.Errors[i];
					Logger.Error(item.ErrorText);
				}
				throw new Exception();
			}
			//throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);

			return Tuple.Create(
					compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
					compileResult.CompiledAssembly);
		}


		/// <summary>
		/// Creates the compile results for the specified <see cref="TypeContext"/>.
		/// </summary>
		/// <param name="context">The type context.</param>
		/// <returns>The compiler results.</returns>
		[Pure]
		private Tuple<CompilerResults, string> Compile(TypeContext context)
		{
			RazorHandler.CleanModelType(context);
			var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,
																					 context.TemplateType, context.ModelType);


			var @params = new CompilerParameters
			{
				GenerateInMemory = true,
				GenerateExecutable = false,
				IncludeDebugInformation = false,
				CompilerOptions = "/target:library /optimize /define:RAZORENGINE"
			};

			var assemblies = CompilerServicesUtility
					.GetLoadedAssemblies()
					.Where(a => !a.IsDynamic && File.Exists(a.Location))
					.GroupBy(a => a.GetName().Name).Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
					.Select(a => a.Location);

			var includeAssemblies = (IncludeAssemblies() ?? Enumerable.Empty<string>());
			assemblies = assemblies.Concat(includeAssemblies)
					.Where(a => !string.IsNullOrWhiteSpace(a))
					.Distinct(StringComparer.InvariantCultureIgnoreCase);

			@params.ReferencedAssemblies.AddRange(assemblies.ToArray());

			string sourceCode = null;
			if (Debug)
			{
				var builder = new StringBuilder();
				using (var writer = new StringWriter(builder, Thread.CurrentThread.CurrentCulture))
				{
					_codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
					sourceCode = builder.ToString();
				}
			}

			return Tuple.Create(_codeDomProvider.CompileAssemblyFromDom(@params, compileUnit), sourceCode);
		}

		private void ParseModelFromContext(TypeContext context)
		{
			var defaultContent = context.TemplateContent;
			var modelIndex = defaultContent.IndexOf("@model ", StringComparison.InvariantCultureIgnoreCase);
			//No model present! :)
			if (modelIndex < 0)
			{
				context.ModelType = typeof(object);
				return;
			}

			var endOfModel = 0;
			var modelLength = 0;
			for (int i = modelIndex; i < defaultContent.Length; i++)
			{
				var selctedChar = defaultContent[i];
				if (selctedChar == '\n' || selctedChar == '\r' || selctedChar == '\f')
				{
					endOfModel = i;
					break;
				}
				modelLength++;
			}
			if (endOfModel == 0) return;
			var modelType = defaultContent.Substring(modelIndex + "@model ".Length, modelLength - "@model ".Length).Trim();
			var realContent = string.Empty;
			if (modelIndex > 0)
			{
				realContent += defaultContent.Substring(0, modelIndex);
			}
			if (context.ModelType == null)
			{
				context.ModelType = InferModelType(modelType);
			}
			realContent += defaultContent.Substring(endOfModel);
			context.TemplateContent = realContent;
		}

		private Type InferModelType(string modelType)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}