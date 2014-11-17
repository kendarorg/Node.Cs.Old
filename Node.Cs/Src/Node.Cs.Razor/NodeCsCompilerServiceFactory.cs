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
using Microsoft.CSharp;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.VisualBasic;

namespace Node.Cs.Razor
{
	public class NodeCsCompilerServiceFactory : ICompilerServiceFactory
	{
		public NodeCsCompilerServiceFactory()
		{
		}
		#region Methods
		/// <summary>
		/// Creates a <see cref="ICompilerService"/> that supports the specified language.
		/// </summary>
		/// <param name="language">The <see cref="Language"/>.</param>
		/// <returns>An instance of <see cref="ICompilerService"/>.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ICompilerService CreateCompilerService(Language language)
		{
			switch (language)
			{
				case Language.CSharp:
					return new NodeCsCSharpDirectCompilerService( codeProvider: new CSharpCodeProvider());

				case Language.VisualBasic:
					return new VBDirectCompilerService();

				default:
					throw new ArgumentException("Unsupported language: " + language);
			}
		}
		#endregion
	}
}