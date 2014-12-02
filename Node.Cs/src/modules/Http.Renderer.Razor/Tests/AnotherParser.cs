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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Razor;
using System.Web.Razor.Parser;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

#if NEVER
//USAGE

//string result = RazorEngine.Razor.Parse(fileContent,new object(), "test");
namespace RazorEngine
{

	/// <summary>
	/// Compiles razor templates.
	/// </summary>
	internal class RazorCompiler
	{
#region Fields
		private readonly IRazorProvider provider;
		#endregion

#region Constructor
		/// <summary>
		/// Initialises a new instance of <see cref="RazorCompiler"/>.
		/// </summary>
		/// <param name="provider">The provider used to compile templates.</param>
		public RazorCompiler(IRazorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			this.provider = provider;
		}
		#endregion

#region Methods
		/// <summary>
		/// Compiles the template.
		/// </summary>
		/// <param name="className">The class name of the dynamic type.</param>
		/// <param name="template">The template to compile.</param>
		/// <param name="modelType">[Optional] The mode type.</param>
		private CompilerResults Compile(string className, string template, Type modelType = null)
		{
			var languageService = provider.CreateLanguageService();
			var codeDom = provider.CreateCodeDomProvider();
			var host = new NodeRazorHost(languageService);

			var generator = languageService.CreateCodeGenerator(className, "Razor.Dynamic", null, host);
			var parser = new RazorParser(languageService.CreateCodeParser(), new HtmlMarkupParser());

			Type baseType = (modelType == null)
					? typeof(TemplateBase)
					: typeof(TemplateBase<>).MakeGenericType(modelType);

			//EDR generator.GeneratedClass.BaseTypes.Add(baseType);

			using (var reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(template))))
			{
				parser.Parse(reader, generator);
			}

			//EDRvar statement = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Clear");
			//EDRgenerator.GeneratedExecuteMethod.Statements.Insert(0, new CodeExpressionStatement(statement));

			var builder = new StringBuilder();
			using (var writer = new StringWriter(builder))
			{
				//EDR codeDom.GenerateCodeFromCompileUnit(generator.GeneratedCode, writer, new CodeGeneratorOptions());
			}

			var @params = new CompilerParameters();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				@params.ReferencedAssemblies.Add(assembly.Location);
			}

			@params.GenerateInMemory = true;
			@params.IncludeDebugInformation = false;
			@params.GenerateExecutable = false;
			@params.CompilerOptions = "/target:library /optimize";

			var result = codeDom.CompileAssemblyFromSource(@params, new[] { builder.ToString() });
			return result;
		}

		/// <summary>
		/// Creates a <see cref="ITemplate" /> from the specified template string.
		/// </summary>
		/// <param name="template">The template to compile.</param>
		/// <param name="modelType">[Optional] The model type.</param>
		/// <returns>An instance of <see cref="ITemplate"/>.</returns>
		public ITemplate CreateTemplate(string template, Type modelType = null)
		{
			string className = Regex.Replace(Guid.NewGuid().ToString("N"), @"[^A-Za-z]*", "");

			var result = Compile(className, template, modelType);

			if (result.Errors != null && result.Errors.Count > 0)
				throw new TemplateException(result.Errors);

			ITemplate instance = (ITemplate)result.CompiledAssembly.CreateInstance("Razor.Dynamic." + className);

			return instance;
		}
#endregion
	}
	/// <summary>
	/// Process razor templates.
	/// </summary>
	public static class Razor
	{
#region Fields
		private static RazorCompiler Compiler;
		private static readonly IDictionary<string, ITemplate> Templates;
#endregion

#region Constructor
		/// <summary>
		/// Statically initialises the <see cref="Razor"/> type.
		/// </summary>
		static Razor()
		{
			Compiler = new RazorCompiler(new CSharpRazorProvider());
			Templates = new Dictionary<string, ITemplate>();
		}
#endregion

#region Methods
		/// <summary>
		/// Gets an <see cref="ITemplate"/> for the specified template.
		/// </summary>
		/// <param name="template">The template to parse.</param>
		/// <param name="modelType">The model to use in the template.</param>
		/// <param name="name">[Optional] The name of the template.</param>
		/// <returns></returns>
		private static ITemplate GetTemplate(string template, Type modelType, string name = null)
		{
			if (!string.IsNullOrEmpty(name))
			{
				if (Templates.ContainsKey(name))
					return Templates[name];
			}

			var instance = Compiler.CreateTemplate(template, modelType);

			if (!string.IsNullOrEmpty(name))
			{
				if (!Templates.ContainsKey(name))
					Templates.Add(name, instance);
			}

			return instance;
		}

		/// <summary>
		/// Parses the specified template using the specified model.
		/// </summary>
		/// <typeparam name="T">The model type.</typeparam>
		/// <param name="template">The template to parse.</param>
		/// <param name="model">The model to use in the template.</param>
		/// <param name="name">[Optional] A name for the template used for caching.</param>
		/// <returns>The parsed template.</returns>
		public static string Parse<T>(string template, T model, string name = null)
		{
			var instance = GetTemplate(template, typeof(T), name);
			if (instance is ITemplate<T>)
				((ITemplate<T>)instance).Model = model;

			instance.Execute();
			return instance.Result;
		}

		/// <summary>
		/// Sets the razor provider used for compiling templates.
		/// </summary>
		/// <param name="provider">The razor provider.</param>
		public static void SetRazorProvider(IRazorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			Compiler = new RazorCompiler(provider);
		}
#endregion
	}
	/// <summary>
	/// A razor template.
	/// </summary>
	public interface ITemplate
	{
#region Properties
		/// <summary>
		/// Gets the parsed result of the template.
		/// </summary>
		string Result { get; }
#endregion

#region Methods
		/// <summary>
		/// Clears the template.
		/// </summary>
		void Clear();

		/// <summary>
		/// Executes the template.
		/// </summary>
		void Execute();

		/// <summary>
		/// Writes the specified object to the template.
		/// </summary>
		/// <param name="object"></param>
		void Write(object @object);

		/// <summary>
		/// Writes a literal to the template.
		/// </summary>
		/// <param name="literal"></param>
		void WriteLiteral(string literal);
#endregion
	}

	/// <summary>
	/// A razor template with a model.
	/// </summary>
	/// <typeparam name="TModel">The model type</typeparam>
	public interface ITemplate<TModel> : ITemplate
	{
#region Properties
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		TModel Model { get; set; }
#endregion
	}
	/// <summary>
	/// Defines a provider used to create associated compiler types.
	/// </summary>
	public interface IRazorProvider
	{
#region Methods
		/// <summary>
		/// Creates a code language service.
		/// </summary>
		/// <returns>Creates a language service.</returns>
		RazorCodeLanguage CreateLanguageService();

		/// <summary>
		/// Creates a <see cref="CodeDomProvider"/>.
		/// </summary>
		/// <returns>The a code dom provider.</returns>
		CodeDomProvider CreateCodeDomProvider();
#endregion
	}
	/// <summary>
	/// Provides a razor provider that supports the C# syntax.
	/// </summary>
	public class CSharpRazorProvider : IRazorProvider
	{
#region Methods
		/// <summary>
		/// Creates a code language service.
		/// </summary>
		/// <returns>Creates a language service.</returns>
		public RazorCodeLanguage CreateLanguageService()
		{
			return new CSharpRazorCodeLanguage();
		}

		/// <summary>
		/// Creates a <see cref="CodeDomProvider"/>.
		/// </summary>
		/// <returns>The a code dom provider.</returns>
		public CodeDomProvider CreateCodeDomProvider()
		{
			return new CSharpCodeProvider();
		}
#endregion
	}
	/// <summary>
	/// Provides a base implementation of a template.
	/// </summary>
	public abstract class TemplateBase : ITemplate
	{
#region Fields
		private readonly StringBuilder builder = new StringBuilder();
#endregion

#region Properties
		/// <summary>
		/// Gets the parsed result of the template.
		/// </summary>
		public string Result { get { return builder.ToString(); } }
#endregion

#region Methods
		/// <summary>
		/// Clears the template.
		/// </summary>
		public void Clear()
		{
			builder.Clear();
		}

		/// <summary>
		/// Executes the template.
		/// </summary>
		public virtual void Execute() { }

		/// <summary>
		/// Writes the specified object to the template.
		/// </summary>
		/// <param name="object"></param>
		public void Write(object @object)
		{
			if (@object == null)
				return;

			builder.Append(@object);
		}

		/// <summary>
		/// Writes a literal to the template.
		/// </summary>
		/// <param name="literal"></param>
		public void WriteLiteral(string literal)
		{
			if (literal == null)
				return;

			builder.Append(literal);
		}
#endregion
	}

	/// <summary>
	/// Provides a base implementation of a template.
	/// </summary>
	/// <typeparam name="TModel">The model type.</typeparam>
	public abstract class TemplateBase<TModel> : TemplateBase, ITemplate<TModel>
	{
#region Properties
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		public TModel Model { get; set; }
#endregion
	}
	/// <summary>
	/// Provides a razor provider that supports the VB syntax.
	/// </summary>
	public class VBRazorProvider : IRazorProvider
	{
#region Methods
		/// <summary>
		/// Creates a code language service.
		/// </summary>
		/// <returns>Creates a language service.</returns>
		public RazorCodeLanguage CreateLanguageService()
		{
			return new VBRazorCodeLanguage();
		}

		/// <summary>
		/// Creates a <see cref="CodeDomProvider"/>.
		/// </summary>
		/// <returns>The a code dom provider.</returns>
		public CodeDomProvider CreateCodeDomProvider()
		{
			return new VBCodeProvider();
		}
#endregion
	}

	public class TemplateException : Exception
	{
#region Constructors
		/// <summary>
		/// Initialises a new instance of <see cref="TemplateException"/>
		/// </summary>
		/// <param name="errors">The collection of compilation errors.</param>
		internal TemplateException(CompilerErrorCollection errors)
			: base("Unable to compile template.")
		{
			var list = new List<CompilerError>();
			foreach (CompilerError error in errors)
			{
				list.Add(error);
			}
			Errors = new ReadOnlyCollection<CompilerError>(list);
		}
#endregion

#region Properties
		/// <summary>
		/// Gets the collection of compiler errors.
		/// </summary>
		public ReadOnlyCollection<CompilerError> Errors { get; private set; }
#endregion
	}
}
#endif