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
using System.Security.Policy;
using System.Security.Principal;
using Node.Cs.Razor;
using Node.Cs.Razor.Helpers;
using Node.Cs.Razor.Rendering;
using RazorEngine.Templating;
using System.Web;
using Node.Cs.Lib.Controllers;


namespace Node.Cs.RazorTemplate
{
	public interface INodeCsTemplateBase<T> : ITemplate<T>
	{
		HttpContextBase Context { get; }
	}

	[RequireNamespaces("Node.Cs.RazorTemplate")]
	public abstract class WebConfigNodeCsTemplateBase : TemplateBase
	{
		private HttpContextBase _context;
		private HttpRequestBase _request;
		private IPrincipal _user;
		private HtmlHelper<object> _htmlHelper;
		private string _localPath;
		private UrlHelper _urlHelper;
		private ViewContext _viewContext;

		public Dictionary<string, object> ViewData { get; set; }

		public String LocalPath
		{
			get
			{
				InitializeHelpers();
				return _localPath;
			}
		}

		public UrlHelper Url
		{
			get
			{
				InitializeHelpers();
				return _urlHelper;
			}
		}


		public HtmlHelper<object> Html
		{
			get
			{
				InitializeHelpers();
				return _htmlHelper;
			}
		}

		public ViewContext ViewContext
		{
			get
			{
				InitializeHelpers();
				return _viewContext;
			}
		}

		public HttpRequestBase Request
		{
			get
			{
				InitializeHelpers();
				return _request;
			}
		}

		public IPrincipal User
		{
			get
			{

				InitializeHelpers();
				return _user;
			}
		}

		public HttpContextBase Context
		{
			get
			{
				InitializeHelpers();
				return _context;
			}
			set
			{
				_context = value;
				InitializeHelpers(true);
			}
		}

		private ModelStateDictionary _modelState;

		private void InitializeHelpers(bool force = false)
		{
			if (_context != null && !force) return;
			_context = ViewBag.NodeCsContext;
			_localPath = ViewBag.NodeCsLocalPath;
			_modelState = ViewBag.ModelState;
			ViewData = ViewBag.ViewData;
			_request = _context.Request;
			_user = _context.User;
			_viewContext = new ViewContext(_context, this, _modelState);
			_urlHelper = new UrlHelper(_context);
			_htmlHelper = new HtmlHelper<object>(_context, ViewContext, this, LocalPath,ViewBag);
			_htmlHelper.Model = new object();
		}
	}

	[RequireNamespaces("Node.Cs.RazorTemplate")]
	public abstract class NodeCsTemplateBase<T> : TemplateBase<T>, INodeCsTemplateBase<T>
	{
		private HttpContextBase _context;
		private HttpRequestBase _request;
		private IPrincipal _user;
		private HtmlHelper<T> _htmlHelper;
		private string _localPath;
		private UrlHelper _urlHelper;
		private ViewContext _viewContext;

		public Dictionary<string, object> ViewData { get; set; }

		public String LocalPath
		{
			get
			{
				InitializeHelpers();
				return _localPath;
			}
		}

		public UrlHelper Url
		{
			get
			{
				InitializeHelpers();
				return _urlHelper;
			}
		}


		public HtmlHelper<T> Html
		{
			get
			{
				InitializeHelpers();
				return _htmlHelper;
			}
		}

		public ViewContext ViewContext
		{
			get
			{
				InitializeHelpers();
				return _viewContext;
			}
		}

		public HttpRequestBase Request
		{
			get
			{
				InitializeHelpers();
				return _request;
			}
		}

		public IPrincipal User
		{
			get
			{

				InitializeHelpers();
				return _user;
			}
		}

		public HttpContextBase Context
		{
			get
			{
				InitializeHelpers();
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		private ModelStateDictionary _modelState;

		private void InitializeHelpers(bool force = false)
		{
			if (_context != null && !force) return;
			_context = ViewBag.NodeCsContext;
			_localPath = ViewBag.NodeCsLocalPath;
			_modelState = ViewBag.ModelState;
			ViewData = ViewBag.ViewData;
			_request = _context.Request;
			_user = _context.User;
			_viewContext = new ViewContext(_context, this, _modelState);
			_urlHelper = new UrlHelper(_context);
			_htmlHelper = new HtmlHelper<T>(_context, ViewContext, this, LocalPath,ViewBag);
			_htmlHelper.Model = Model;
		}
	}
}
