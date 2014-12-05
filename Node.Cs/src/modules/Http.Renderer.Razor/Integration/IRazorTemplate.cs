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
using Http.Renderer.Razor.Utils;

namespace Http.Renderer.Razor.Integration
{
	public interface IRazorTemplate
	{
		List<BufferItem> Buffer { get; }
		
		#region MVC complient
		string Layout { get; set; }
		dynamic ViewBag { get; }
		RawString RenderBody();

		// you may skip the layout using the parameter skipLayoutwhich can be handy 
		// when rendering template partials (controls)
		RawString RenderPage(string name, object model = null, bool skipLayout = false);

		RawString RenderSection(string sectionName, bool required = false);
		bool IsSectionDefined(string sectionName);
		#endregion

		#region Added through GeneratedClassContext
		void DefineSection(string name, Action action);
		void WriteLiteralTo(TextWriter writer, object value);
		void WriteTo(TextWriter writer, object value);
		void Write(object value);
		void WriteLiteral(object value);
		void Execute();
		#endregion
	}
}