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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Node.Cs.Lib.Contexts.ContentUtils;

namespace Node.Cs.Lib.Contexts
{
	public class NodeCsRequest : HttpRequestBase
	{
		private NodeCsHttpFileCollection _files;
		private readonly HttpListenerRequest _request;
		private readonly NodeCsContext _context;
		private Encoding _contentEncoding;
		private NameValueCollection _queryString;
		private NameValueCollection _form;
		private Stream _inputStream;

		public override bool IsAuthenticated
		{
			get { return _context.User != null; }
		}

		public NodeCsRequest(Uri url)
		{
			_url = url;
		}

		public NodeCsRequest(HttpListenerRequest request, NodeCsContext context)
		{
			Initialized = false;
			_queryString = new NameValueCollection();
			_form = new NameValueCollection();
			_files = new NodeCsHttpFileCollection(new Dictionary<string, NodeCsFile>(StringComparer.OrdinalIgnoreCase));
			_request = request;
			_context = context;
			_url = _request.Url;
			try
			{
				_contentEncoding = _request.ContentEncoding;
			}
			catch
			{
				_contentEncoding = Encoding.UTF8;
			}
			_queryString = HttpUtility.ParseQueryString(_request.Url.Query);
			
		}

		public void Initialize()
		{
			FillInFormCollection();
			FillInCookies();
		}

		private HttpCookieCollection _cookies;
		private void FillInCookies()
		{
			_cookies = new HttpCookieCollection();
			foreach (Cookie cookie in _request.Cookies)
			{
				_cookies.Add(new HttpCookie(cookie.Name, cookie.Value)
									 {
										 //Domain = cookie.Domain,
										 Expires = cookie.Expires,
										 HttpOnly = cookie.HttpOnly,
										 Path = "/",
										 Secure = cookie.Secure
									 });
			}
		}

		public override HttpCookieCollection Cookies
		{
			get { return _cookies; }
		}


		public bool Initialized { get; set; }

		public void Initialize(HttpListenerRequest request)
		{
			try
			{
				_contentEncoding = _request.ContentEncoding;
			}
			catch
			{
				_contentEncoding = Encoding.UTF8;
			}
			_queryString = HttpUtility.ParseQueryString(_request.Url.Query);
			FillInFormCollection();
			FillInCookies();
		}

		public NodeCsRequest()
		{
			Initialized = false;
			_queryString = new NameValueCollection();
			_form = new NameValueCollection();
			_files = new NodeCsHttpFileCollection(new Dictionary<string, NodeCsFile>(StringComparer.OrdinalIgnoreCase));
		}



		public override NameValueCollection Headers
		{
			get { return _request.Headers; }
		}

		public override Stream InputStream
		{
			get
			{
				return _inputStream;
			}
		}

		private void FillInFormCollection()
		{
			_form = new NameValueCollection();

			if (ContentType == null)
			{
				_inputStream = new MemoryStream(new byte[] { });
				_files = new NodeCsHttpFileCollection(new Dictionary<string, NodeCsFile>());
				return;
			}

			if (ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
			{
				var formUlrEncoded = new UrlEncodedStreamConverter();
				formUlrEncoded.Initialize(_request.InputStream, ContentEncoding, ContentType);
				foreach (var key in formUlrEncoded.Keys)
				{
					_form.Add(key,HttpUtility.UrlDecode(formUlrEncoded[key]));
				}
			}
			else if (ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
			{
				var multipartForm = new MultipartFormStreamConverter();
				multipartForm.Initialize(_request.InputStream, ContentEncoding, ContentType);
				foreach (var key in multipartForm.Keys)
				{
					_form.Add(key, multipartForm[key]);
				}
				var filesDictionary = new Dictionary<string, NodeCsFile>(StringComparer.OrdinalIgnoreCase);
				foreach (var file in multipartForm.Files)
				{
					var stream = new MemoryStream();
					stream.Write(multipartForm.Content, file.Start, file.Length);
					filesDictionary.Add(file.Name, new NodeCsFile(file.FileName, stream, file.ContentType));
				}
				_files = new NodeCsHttpFileCollection(filesDictionary);
			}
			else
			{
				_inputStream = _request.InputStream;
			}
		}


		public override NameValueCollection QueryString
		{
			get { return _queryString; }
		}

		public override NameValueCollection Form
		{
			get { return _form; }
		}

		public override string ContentType
		{
			get
			{
				return _request.ContentType;
			}
			set
			{

			}
		}

		public override string HttpMethod
		{
			get
			{
				return _request.HttpMethod;
			}
		}

		public override Uri Url { get { return _url; } }
		public override Uri UrlReferrer { get { return _request.UrlReferrer; } }

		public override Encoding ContentEncoding { get { return _contentEncoding; } set { _contentEncoding = value; } }

		public override HttpFileCollectionBase Files
		{
			get
			{
				return _files;
			}
		}

		private NameValueCollection _params;
		private Uri _url;

		public void ForceUrl(Uri url)
		{
			_url = url;
		}

		public override NameValueCollection Params
		{
			get
			{
				if (_params != null) return _params;

				var parameters = new NameValueCollection();
				foreach (var item in Form.AllKeys)
				{
					parameters.Add(item, Form[item]);
				}
				foreach (var item in QueryString.AllKeys)
				{
					parameters.Add(item, QueryString[item]);
				}
				_params = parameters;
				return _params;
			}
		}

		public override bool IsLocal
		{
			get { return _request.IsLocal; }
		}
	}
}
