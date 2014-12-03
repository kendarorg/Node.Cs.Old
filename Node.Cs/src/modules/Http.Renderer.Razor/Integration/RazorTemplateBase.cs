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


using System.Dynamic;
using CoroutinesLib.Shared;
using Http.Contexts;
using Http.Renderer.Razor.Helpers;
using Http.Shared.Contexts;
using HttpMvc.Controllers;
using NodeCs.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Http.Renderer.Razor.Integration
{


	public abstract class RazorTemplateBase : IRazorTemplate
	{
		public IHttpContext Context { get; set; }
		public List<BufferItem> Buffer { get; private set; }
		public string Layout { get; set; }
		public dynamic ViewBag { get; set; }
		public object ObjectModel { get; set; }

		protected Dictionary<string, Action> _sections = new Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase);

		public string RenderBody()
		{
			throw new NotImplementedException();
		}

		public string RenderPage(string name, object model = null, bool skipLayout = false)
		{
			var http = ServiceLocator.Locator.Resolve<HttpModule>();

			var context = new WrappedHttpContext(Context);
			
			var newUrl = name.TrimStart('~');

			((IHttpRequest)context.Request).SetUrl(new Uri(newUrl,UriKind.Relative));
			((IHttpRequest)context.Request).SetQueryString(Context.Request.QueryString);
			var internalCoroutine = http.SetupInternalRequestCoroutine(context, model, ViewBag);
			var task = CoroutineResult.WaitForCoroutine(internalCoroutine);
			task.Wait();
			var stream = context.Response.OutputStream as MemoryStream;

			// ReSharper disable once PossibleNullReferenceException
			var result = Encoding.UTF8.GetString(stream.ToArray());
			return result;
		}

		public string RenderSection(string sectionName, bool required = false)
		{
			if (IsSectionDefined(sectionName))
			{
				_sections[sectionName]();
			}
			return string.Empty;
		}

		public bool IsSectionDefined(string sectionName)
		{
			return _sections.ContainsKey(sectionName);
		}

		public void DefineSection(string name, Action action)
		{
			_sections[name] = action;
		}

		public void WriteLiteralTo(TextWriter writer, object value)
		{
			writer.Write(value);
		}

		public void WriteTo(TextWriter writer, object value)
		{
			writer.Write(value);
		}

		protected RazorTemplateBase()
		{
			Buffer = new List<BufferItem>();
		}

		public abstract void Execute();

		public virtual void Write(object value)
		{
			WriteLiteral(value);
		}

		public virtual void WriteLiteral(object value)
		{
			Buffer.Add(new BufferItem { Value = value });
		}

	}
	public abstract class RazorTemplateBase<T> : RazorTemplateBase
	{
		public HtmlHelper<T> Html { get; set; }
		public UrlHelper Url { get; set; }
		public T Model
		{
			get { return (T)ObjectModel; }
			set { ObjectModel = (T)value; }
		}

		// ReSharper disable InconsistentNaming
		public T model
		{
			get { return Model; }
			set { Model = value; }
		}
		// ReSharper restore InconsistentNaming
	}
}