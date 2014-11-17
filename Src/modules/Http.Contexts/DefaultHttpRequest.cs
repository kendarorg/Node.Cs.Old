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


using System.Collections.Generic;
using System.Web;
using System;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web.Routing;
using System.Threading;
using Http.Contexts.GenericUtils;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class DefaultHttpRequest : HttpRequestBase, IHttpRequest
	{
		public DefaultHttpRequest() { }

		private readonly HttpRequest _httpRequest;
		public object SourceObject { get { return _httpRequest; } }
		public DefaultHttpRequest(HttpRequest httpRequest)
		{
			_httpRequest = httpRequest;
			InitializeUnsettable();
		}

		public override void Abort()
		{
			_httpRequest.Abort();
		}

		public override Byte[] BinaryRead(Int32 count)
		{
			return _httpRequest.BinaryRead(count);
		}

		public override Stream GetBufferedInputStream()
		{
			return _httpRequest.GetBufferedInputStream();
		}

		public override Stream GetBufferlessInputStream()
		{
			return _httpRequest.GetBufferlessInputStream();
		}

		public override Stream GetBufferlessInputStream(Boolean disableMaxRequestLength)
		{
			return _httpRequest.GetBufferlessInputStream(disableMaxRequestLength);
		}

		public override void InsertEntityBody()
		{
			_httpRequest.InsertEntityBody();
		}

		public override void InsertEntityBody(Byte[] buffer, Int32 offset, Int32 count)
		{
			_httpRequest.InsertEntityBody(buffer, offset, count);
		}

		public override Int32[] MapImageCoordinates(String imageFieldName)
		{
			return _httpRequest.MapImageCoordinates(imageFieldName);
		}

		public override Double[] MapRawImageCoordinates(String imageFieldName)
		{
			return _httpRequest.MapRawImageCoordinates(imageFieldName);
		}

		public override String MapPath(String virtualPath)
		{
			return _httpRequest.MapPath(virtualPath);
		}

		public override String MapPath(String virtualPath, String baseVirtualDir, Boolean allowCrossAppMapping)
		{
			return _httpRequest.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
		}

		public override void ValidateInput()
		{
			_httpRequest.ValidateInput();
		}

		public override void SaveAs(String filename, Boolean includeHeaders)
		{
			_httpRequest.SaveAs(filename, includeHeaders);
		}

		public override String[] AcceptTypes { get { return _httpRequest.AcceptTypes; } }

		public Dictionary<string, string> ItemDictionary { get { throw new NotImplementedException(); } }

		public void SetAcceptTypes(String[] val)
		{
		}


		public override String ApplicationPath { get { return _httpRequest.ApplicationPath; } }

		public void SetApplicationPath(String val)
		{
		}


		public override String AnonymousID { get { return _httpRequest.AnonymousID; } }

		public void SetAnonymousID(String val)
		{
		}


		public override String AppRelativeCurrentExecutionFilePath { get { return _httpRequest.AppRelativeCurrentExecutionFilePath; } }

		public void SetAppRelativeCurrentExecutionFilePath(String val)
		{
		}




		public override ChannelBinding HttpChannelBinding { get { return _httpRequest.HttpChannelBinding; } }

		public void SetHttpChannelBinding(ChannelBinding val)
		{
		}


		public override HttpClientCertificate ClientCertificate { get { return _httpRequest.ClientCertificate; } }

		public void SetClientCertificate(HttpClientCertificate val)
		{
		}

		public override Encoding ContentEncoding
		{
			set
			{
				_httpRequest.ContentEncoding = value;
			}
			get
			{
				return _httpRequest.ContentEncoding;
			}
		}


		public override Int32 ContentLength { get { return _httpRequest.ContentLength; } }

		public void SetContentLength(Int32 val)
		{
		}

		public override String ContentType
		{
			set
			{
				_httpRequest.ContentType = value;
			}
			get
			{
				return _httpRequest.ContentType;
			}
		}


		public override HttpCookieCollection Cookies { get { return _httpRequest.Cookies; } }

		public void SetCookies(HttpCookieCollection val)
		{
		}


		public override String CurrentExecutionFilePath { get { return _httpRequest.CurrentExecutionFilePath; } }

		public void SetCurrentExecutionFilePath(String val)
		{
		}


		public override String CurrentExecutionFilePathExtension { get { return _httpRequest.CurrentExecutionFilePathExtension; } }

		public void SetCurrentExecutionFilePathExtension(String val)
		{
		}


		public override String FilePath { get { return _httpRequest.FilePath; } }

		public void SetFilePath(String val)
		{
		}

		public override Stream Filter
		{
			set
			{
				_httpRequest.Filter = value;
			}
			get
			{
				return _httpRequest.Filter;
			}
		}


		public override NameValueCollection Form { get { return _httpRequest.Form; } }

		public void SetForm(NameValueCollection val)
		{
		}


		public override String HttpMethod { get { return _httpRequest.HttpMethod; } }

		public void SetHttpMethod(String val)
		{
		}


		public override Stream InputStream { get { return _httpRequest.InputStream; } }

		public void SetInputStream(Stream val)
		{
		}


		public override Boolean IsAuthenticated { get { return _httpRequest.IsAuthenticated; } }

		public void SetIsAuthenticated(Boolean val)
		{
		}


		public override Boolean IsLocal { get { return _httpRequest.IsLocal; } }

		public void SetIsLocal(Boolean val)
		{
		}


		public override Boolean IsSecureConnection { get { return _httpRequest.IsSecureConnection; } }

		public void SetIsSecureConnection(Boolean val)
		{
		}


		public override WindowsIdentity LogonUserIdentity { get { return _httpRequest.LogonUserIdentity; } }

		public void SetLogonUserIdentity(WindowsIdentity val)
		{
		}


		public override NameValueCollection Params { get { return _httpRequest.Params; } }

		public void SetParams(NameValueCollection val)
		{
		}


		public override String Path { get { return _httpRequest.Path; } }

		public void SetPath(String val)
		{
		}


		public override String PathInfo { get { return _httpRequest.PathInfo; } }

		public void SetPathInfo(String val)
		{
		}


		public override String PhysicalApplicationPath { get { return _httpRequest.PhysicalApplicationPath; } }

		public void SetPhysicalApplicationPath(String val)
		{
		}


		public override String PhysicalPath { get { return _httpRequest.PhysicalPath; } }

		public void SetPhysicalPath(String val)
		{
		}


		public override String RawUrl { get { return _httpRequest.RawUrl; } }

		public void SetRawUrl(String val)
		{
		}


		public override ReadEntityBodyMode ReadEntityBodyMode { get { return _httpRequest.ReadEntityBodyMode; } }

		public void SetReadEntityBodyMode(ReadEntityBodyMode val)
		{
		}

		public override RequestContext RequestContext
		{
			set
			{
				_httpRequest.RequestContext = value;
			}
			get
			{
				return _httpRequest.RequestContext;
			}
		}

		public override String RequestType
		{
			set
			{
				_httpRequest.RequestType = value;
			}
			get
			{
				return _httpRequest.RequestType;
			}
		}


		public override NameValueCollection ServerVariables { get { return _httpRequest.ServerVariables; } }

		public void SetServerVariables(NameValueCollection val)
		{
		}


		public override CancellationToken TimedOutToken { get { return _httpRequest.TimedOutToken; } }

		public void SetTimedOutToken(CancellationToken val)
		{
		}


		public override Int32 TotalBytes { get { return _httpRequest.TotalBytes; } }

		public void SetTotalBytes(Int32 val)
		{
		}




		public override Uri Url { get { return _httpRequest.Url; } }

		public void SetUrl(Uri val)
		{
		}


		public override Uri UrlReferrer { get { return _httpRequest.UrlReferrer; } }

		public void SetUrlReferrer(Uri val)
		{
		}


		public override String UserAgent { get { return _httpRequest.UserAgent; } }

		public void SetUserAgent(String val)
		{
		}


		public override String[] UserLanguages { get { return _httpRequest.UserLanguages; } }

		public void SetUserLanguages(String[] val)
		{
		}


		public override String UserHostAddress { get { return _httpRequest.UserHostAddress; } }

		public void SetUserHostAddress(String val)
		{
		}


		public override String UserHostName { get { return _httpRequest.UserHostName; } }

		public void SetUserHostName(String val)
		{
		}


		public override NameValueCollection Headers { get { return _httpRequest.Headers; } }

		public void SetHeaders(NameValueCollection val)
		{
		}


		public override NameValueCollection QueryString { get { return _httpRequest.QueryString; } }

		public void SetQueryString(NameValueCollection val)
		{
		}

		private HttpBrowserCapabilitiesBase _browser;

		public override HttpBrowserCapabilitiesBase Browser { get { return _browser; } }

		public void SetBrowser(HttpBrowserCapabilitiesBase val)
		{
		}

		private HttpFileCollectionBase _files;

		public override HttpFileCollectionBase Files { get { return _files; } }

		public void SetFiles(HttpFileCollectionBase val)
		{
		}

		private UnvalidatedRequestValuesBase _unvalidated;
		public override UnvalidatedRequestValuesBase Unvalidated { get { return _unvalidated; } }

		public void SetUnvalidated(UnvalidatedRequestValuesBase val)
		{
		}

		public override string this[string key]
		{
			get { return _httpRequest[key]; }
		}

		public void InitializeUnsettable()
		{
			_browser = ConvertBrowser(_httpRequest.Browser);
			_files = ConvertFiles(_httpRequest.Files);
			_unvalidated = ConvertUnvalidated(_httpRequest.Unvalidated);
		}

		// ReSharper disable once UnusedParameter.Local
		private UnvalidatedRequestValuesBase ConvertUnvalidated(UnvalidatedRequestValues unvalidated)
		{
			return null;
		}

		// ReSharper disable once UnusedParameter.Local
		private HttpBrowserCapabilitiesBase ConvertBrowser(HttpBrowserCapabilities browser)
		{
			return null;
		}

		private HttpFileCollectionBase ConvertFiles(HttpFileCollection files)
		{
			var dict = new Dictionary<string, PostedFile>();

			foreach (var file in files.AllKeys)
			{
				dict.Add(file, new PostedFile(files[file]));
			}
			return new FileCollection(dict);
		}
	}
}