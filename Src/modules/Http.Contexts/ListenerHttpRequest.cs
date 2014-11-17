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
using System.Net;
using System;
using System.IO;
using System.Web;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web.Routing;
using System.Threading;
using Http.Contexts.ContentUtils;
using Http.Contexts.GenericUtils;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class ListenerHttpRequest : HttpRequestBase, IHttpRequest
	{
		public ListenerHttpRequest() { }

		public object SourceObject { get { return _httpListenerRequest; } }

		private readonly HttpListenerRequest _httpListenerRequest;

		public ListenerHttpRequest(HttpListenerRequest httpListenerRequest)
		{
			_httpListenerRequest = httpListenerRequest;
			InitializeUnsettable();
		}

		public override void Abort()
		{
			//TODO Missing Abort for HttpListenerRequest
		}

		public override Byte[] BinaryRead(Int32 count)
		{
			//TODO Missing BinaryRead for HttpListenerRequest
			return null;
		}

		public override Stream GetBufferedInputStream()
		{
			//TODO Missing GetBufferedInputStream for HttpListenerRequest
			return null;
		}

		public override Stream GetBufferlessInputStream()
		{
			//TODO Missing GetBufferlessInputStream for HttpListenerRequest
			return null;
		}

		public override Stream GetBufferlessInputStream(Boolean disableMaxRequestLength)
		{
			//TODO Missing GetBufferlessInputStream for HttpListenerRequest
			return null;
		}

		public override void InsertEntityBody()
		{
			//TODO Missing InsertEntityBody for HttpListenerRequest
		}

		public override void InsertEntityBody(Byte[] buffer, Int32 offset, Int32 count)
		{
			//TODO Missing InsertEntityBody for HttpListenerRequest
		}

		public override Int32[] MapImageCoordinates(String imageFieldName)
		{
			//TODO Missing MapImageCoordinates for HttpListenerRequest
			return null;
		}

		public override Double[] MapRawImageCoordinates(String imageFieldName)
		{
			//TODO Missing MapRawImageCoordinates for HttpListenerRequest
			return null;
		}

		public override String MapPath(String virtualPath)
		{
			//TODO Missing MapPath for HttpListenerRequest
			return null;
		}

		public override String MapPath(String virtualPath, String baseVirtualDir, Boolean allowCrossAppMapping)
		{
			//TODO Missing MapPath for HttpListenerRequest
			return null;
		}

		public override void ValidateInput()
		{
			//TODO Missing ValidateInput for HttpListenerRequest
		}

		public override void SaveAs(String filename, Boolean includeHeaders)
		{
			//TODO Missing SaveAs for HttpListenerRequest
		}

		public override String[] AcceptTypes { get { return _httpListenerRequest.AcceptTypes; } }

		public void SetAcceptTypes(String[] val)
		{
		}


		private String _applicationPath;
		public override String ApplicationPath { get { return _applicationPath; } }

		public void SetApplicationPath(String val)
		{
			_applicationPath = val;
		}


		private String _anonymousID;
		public override String AnonymousID { get { return _anonymousID; } }

		public void SetAnonymousID(String val)
		{
			_anonymousID = val;
		}


		private String _appRelativeCurrentExecutionFilePath;
		public override String AppRelativeCurrentExecutionFilePath { get { return _appRelativeCurrentExecutionFilePath; } }

		public void SetAppRelativeCurrentExecutionFilePath(String val)
		{
			_appRelativeCurrentExecutionFilePath = val;
		}


		private HttpBrowserCapabilitiesBase _browser;
		public override HttpBrowserCapabilitiesBase Browser { get { return _browser; } }

		public void SetBrowser(HttpBrowserCapabilitiesBase val)
		{
			_browser = val;
		}


		private ChannelBinding _httpChannelBinding;
		public override ChannelBinding HttpChannelBinding { get { return _httpChannelBinding; } }

		public void SetHttpChannelBinding(ChannelBinding val)
		{
			_httpChannelBinding = val;
		}


		private HttpClientCertificate _clientCertificate;
		public override HttpClientCertificate ClientCertificate { get { return _clientCertificate; } }

		public void SetClientCertificate(HttpClientCertificate val)
		{
			_clientCertificate = val;
		}

		private Encoding _contentEncoding;
		public override Encoding ContentEncoding
		{
			set
			{
				_contentEncoding = value;
			}
			get
			{
				if (_contentEncoding != null) return _contentEncoding;
				return _httpListenerRequest.ContentEncoding;
			}
		}


		private Int32 _contentLength;
		public override Int32 ContentLength { get { return _contentLength; } }

		public void SetContentLength(Int32 val)
		{
			_contentLength = val;
		}

		private String _contentType;
		public override String ContentType
		{
			set
			{
				_contentType = value;
			}
			get
			{
				if (_contentType != null) return _contentType;
				return _httpListenerRequest.ContentType;
			}
		}


		private String _currentExecutionFilePath;
		public override String CurrentExecutionFilePath { get { return _currentExecutionFilePath; } }

		public void SetCurrentExecutionFilePath(String val)
		{
			_currentExecutionFilePath = val;
		}


		private String _currentExecutionFilePathExtension;
		public override String CurrentExecutionFilePathExtension { get { return _currentExecutionFilePathExtension; } }

		public void SetCurrentExecutionFilePathExtension(String val)
		{
			_currentExecutionFilePathExtension = val;
		}


		private String _filePath;
		public override String FilePath { get { return _filePath; } }

		public void SetFilePath(String val)
		{
			_filePath = val;
		}


		private HttpFileCollectionBase _files;
		public override HttpFileCollectionBase Files { get { return _files; } }

		public void SetFiles(HttpFileCollectionBase val)
		{
			_files = val;
		}

		public override Stream Filter { get; set; }


		private NameValueCollection _form;
		public override NameValueCollection Form { get { return _form; } }

		public void SetForm(NameValueCollection val)
		{
			_form = val;
		}


		public override String HttpMethod { get { return _httpListenerRequest.HttpMethod; } }

		public void SetHttpMethod(String val)
		{
		}




		public override Boolean IsAuthenticated { get { return _httpListenerRequest.IsAuthenticated; } }

		public void SetIsAuthenticated(Boolean val)
		{
		}


		public override Boolean IsLocal { get { return _httpListenerRequest.IsLocal; } }

		public void SetIsLocal(Boolean val)
		{
		}


		public override Boolean IsSecureConnection { get { return _httpListenerRequest.IsSecureConnection; } }

		public void SetIsSecureConnection(Boolean val)
		{
		}


		private WindowsIdentity _logonUserIdentity;
		public override WindowsIdentity LogonUserIdentity { get { return _logonUserIdentity; } }

		public void SetLogonUserIdentity(WindowsIdentity val)
		{
			_logonUserIdentity = val;
		}


		private NameValueCollection _params;
		public override NameValueCollection Params { get { return _params; } }

		public void SetParams(NameValueCollection val)
		{
			_params = val;
		}


		private String _path;
		public override String Path { get { return _path; } }

		public void SetPath(String val)
		{
			_path = val;
		}


		private String _pathInfo;
		public override String PathInfo { get { return _pathInfo; } }

		public void SetPathInfo(String val)
		{
			_pathInfo = val;
		}


		private String _physicalApplicationPath;
		public override String PhysicalApplicationPath { get { return _physicalApplicationPath; } }

		public void SetPhysicalApplicationPath(String val)
		{
			_physicalApplicationPath = val;
		}


		private String _physicalPath;
		public override String PhysicalPath { get { return _physicalPath; } }

		public void SetPhysicalPath(String val)
		{
			_physicalPath = val;
		}


		public override String RawUrl { get { return _httpListenerRequest.RawUrl; } }

		public void SetRawUrl(String val)
		{
		}


		private ReadEntityBodyMode _readEntityBodyMode;
		public override ReadEntityBodyMode ReadEntityBodyMode { get { return _readEntityBodyMode; } }

		public void SetReadEntityBodyMode(ReadEntityBodyMode val)
		{
			_readEntityBodyMode = val;
		}

		public override RequestContext RequestContext { get; set; }

		public override string RequestType { get; set; }


		private NameValueCollection _serverVariables;
		public override NameValueCollection ServerVariables { get { return _serverVariables; } }

		public void SetServerVariables(NameValueCollection val)
		{
			_serverVariables = val;
		}


		private CancellationToken _timedOutToken;
		public override CancellationToken TimedOutToken { get { return _timedOutToken; } }

		public void SetTimedOutToken(CancellationToken val)
		{
			_timedOutToken = val;
		}


		private Int32 _totalBytes;
		public override Int32 TotalBytes { get { return _totalBytes; } }

		public void SetTotalBytes(Int32 val)
		{
			_totalBytes = val;
		}


		private UnvalidatedRequestValuesBase _unvalidated;
		public override UnvalidatedRequestValuesBase Unvalidated { get { return _unvalidated; } }

		public void SetUnvalidated(UnvalidatedRequestValuesBase val)
		{
			_unvalidated = val;
		}


		public override Uri Url { get { return _httpListenerRequest.Url; } }

		public void SetUrl(Uri val)
		{
		}


		public override Uri UrlReferrer { get { return _httpListenerRequest.UrlReferrer; } }

		public void SetUrlReferrer(Uri val)
		{
		}


		public override String UserAgent { get { return _httpListenerRequest.UserAgent; } }

		public void SetUserAgent(String val)
		{
		}


		public override String[] UserLanguages { get { return _httpListenerRequest.UserLanguages; } }

		public void SetUserLanguages(String[] val)
		{
		}


		public override String UserHostAddress { get { return _httpListenerRequest.UserHostAddress; } }

		public void SetUserHostAddress(String val)
		{
		}


		public override String UserHostName { get { return _httpListenerRequest.UserHostName; } }

		public void SetUserHostName(String val)
		{
		}


		public override NameValueCollection Headers { get { return _httpListenerRequest.Headers; } }

		public void SetHeaders(NameValueCollection val)
		{
		}


		public override NameValueCollection QueryString { get { return _httpListenerRequest.QueryString; } }

		public void SetQueryString(NameValueCollection val)
		{
		}


		private Dictionary<string, string> _item = new Dictionary<string, string>();

		public override string this[string key]
		{
			get { return _item[key]; }
		}

		public Dictionary<string, string> ItemDictionary
		{
			get { return _item; }
		}

		private HttpCookieCollection _cookies;
		public override HttpCookieCollection Cookies { get { return _cookies; } }

		public void SetCookies(HttpCookieCollection val)
		{
		}

		public void InitializeUnsettable()
		{
			_form = new NameValueCollection();
			_cookies = ConvertCookies(_httpListenerRequest.Cookies);
			//_applicationPath=_httpListenerRequest.ApplicationPath;
			//_anonymousID=_httpListenerRequest.AnonymousID;
			//_appRelativeCurrentExecutionFilePath=_httpListenerRequest.AppRelativeCurrentExecutionFilePath;
			//_browser=_httpListenerRequest.Browser;
			//_httpChannelBinding=_httpListenerRequest.HttpChannelBinding;
			//_clientCertificate=_httpListenerRequest.ClientCertificate;
			//_contentLength=_httpListenerRequest.ContentLength;
			//_currentExecutionFilePath=_httpListenerRequest.CurrentExecutionFilePath;
			//_currentExecutionFilePathExtension=_httpListenerRequest.CurrentExecutionFilePathExtension;
			//_filePath=_httpListenerRequest.FilePath;
			//_files=_httpListenerRequest.Files;
			//_form=_httpListenerRequest.Form;
			//_logonUserIdentity=_httpListenerRequest.LogonUserIdentity;
			//_params=_httpListenerRequest.Params;
			//_path=_httpListenerRequest.Path;
			//_pathInfo=_httpListenerRequest.PathInfo;
			//_physicalApplicationPath=_httpListenerRequest.PhysicalApplicationPath;
			//_physicalPath=_httpListenerRequest.PhysicalPath;
			//_readEntityBodyMode=_httpListenerRequest.ReadEntityBodyMode;
			//_serverVariables=_httpListenerRequest.ServerVariables;
			//_timedOutToken=_httpListenerRequest.TimedOutToken;
			//_totalBytes=_httpListenerRequest.TotalBytes;
			//_unvalidated=_httpListenerRequest.Unvalidated;
			//_item=_httpListenerRequest.Item;
			InitializeFiles();
			_params = new NameValueCollection();
			foreach (var item in _form.AllKeys)
			{
				_params.Add(item, _form[item]);
			}
			foreach (var item in _httpListenerRequest.QueryString.AllKeys)
			{
				_params.Add(item, _httpListenerRequest.QueryString[item]);
			}
		}
		public override Stream InputStream { get { return _inputStream; } }

		public void SetInputStream(Stream val)
		{
		}

		private Stream _inputStream;

		private void InitializeFiles()
		{
			_form = new NameValueCollection();

			if (ContentType == null)
			{
				_inputStream = _httpListenerRequest.InputStream;
				_files = new FileCollection(new Dictionary<string, PostedFile>());
				return;
			}

			if (ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
			{
				var formUlrEncoded = new UrlEncodedStreamConverter();
				formUlrEncoded.Initialize(_httpListenerRequest.InputStream, ContentEncoding, ContentType);
				foreach (var key in formUlrEncoded.Keys)
				{
					var tmp = HttpUtility.UrlDecode(formUlrEncoded[key]);
					if (tmp != null)
					{
						_form.Add(key, tmp);
					}
				}
			}
			else if (ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
			{
				var multipartForm = new MultipartFormStreamConverter();
				multipartForm.Initialize(_httpListenerRequest.InputStream, ContentEncoding, ContentType);
				foreach (var key in multipartForm.Keys)
				{
					_form.Add(key, multipartForm[key]);
				}
				var filesDictionary = new Dictionary<string, PostedFile>(StringComparer.OrdinalIgnoreCase);
				foreach (var file in multipartForm.Files)
				{
					var stream = new MemoryStream();
					stream.Write(multipartForm.Content, file.Start, file.Length);
					filesDictionary.Add(file.Name, new PostedFile(file.FileName, stream, file.ContentType));
				}
				_files = new FileCollection(filesDictionary);
			}
			else
			{
				_inputStream = _httpListenerRequest.InputStream;
			}
		}

		private HttpCookieCollection ConvertCookies(CookieCollection cookies)
		{
			var cc = new HttpCookieCollection();
			foreach (Cookie cookie in cookies)
			{
				cc.Add(new HttpCookie(cookie.Name, cookie.Value)
				{
					//Domain = cookie.Domain,
					Expires = cookie.Expires,
					HttpOnly = cookie.HttpOnly,
					Path = "/",
					Secure = cookie.Secure
				});
			}
			return cc;
		}

	}
}