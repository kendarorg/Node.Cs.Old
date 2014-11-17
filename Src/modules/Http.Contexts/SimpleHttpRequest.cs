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


using Http.Contexts.ContentUtils;
using Http.Contexts.GenericUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class SimpleHttpRequest : HttpRequestBase, IHttpRequest
	{
		public object SourceObject
		{
			get { return null; }
		}

		public override void Abort()
		{
		}

		public override Byte[] BinaryRead(Int32 count)
		{
			return null;
		}

		public override Stream GetBufferedInputStream()
		{
			return null;
		}

		public override Stream GetBufferlessInputStream()
		{
			return null;
		}

		public override Stream GetBufferlessInputStream(Boolean disableMaxRequestLength)
		{
			return null;
		}

		public override void InsertEntityBody()
		{
		}

		public override void InsertEntityBody(Byte[] buffer, Int32 offset, Int32 count)
		{
		}

		public override Int32[] MapImageCoordinates(String imageFieldName)
		{
			return null;
		}

		public override Double[] MapRawImageCoordinates(String imageFieldName)
		{
			return null;
		}

		public override String MapPath(String virtualPath)
		{
			return null;
		}

		public override String MapPath(String virtualPath, String baseVirtualDir, Boolean allowCrossAppMapping)
		{
			return null;
		}

		public override void ValidateInput()
		{
		}

		public override void SaveAs(String filename, Boolean includeHeaders)
		{
		}

		private String[] _acceptTypes = { };

		public override String[] AcceptTypes
		{
			get { return _acceptTypes; }
		}

		public void SetAcceptTypes(String[] val)
		{
			_acceptTypes = val;
		}

		private String _applicationPath = "";

		public override String ApplicationPath
		{
			get { return _applicationPath; }
		}

		public void SetApplicationPath(String val)
		{
			_applicationPath = val;
		}

		private String _anonymousID = "";

		public override String AnonymousID
		{
			get { return _anonymousID; }
		}

		public void SetAnonymousID(String val)
		{
			_anonymousID = val;
		}

		private String _appRelativeCurrentExecutionFilePath = "";

		public override String AppRelativeCurrentExecutionFilePath
		{
			get { return _appRelativeCurrentExecutionFilePath; }
		}

		public void SetAppRelativeCurrentExecutionFilePath(String val)
		{
			_appRelativeCurrentExecutionFilePath = val;
		}

		private HttpBrowserCapabilitiesBase _browser;

		public override HttpBrowserCapabilitiesBase Browser
		{
			get { return _browser; }
		}

		public void SetBrowser(HttpBrowserCapabilitiesBase val)
		{
			_browser = val;
		}

		private ChannelBinding _httpChannelBinding;

		public override ChannelBinding HttpChannelBinding
		{
			get { return _httpChannelBinding; }
		}

		public void SetHttpChannelBinding(ChannelBinding val)
		{
			_httpChannelBinding = val;
		}

		private HttpClientCertificate _clientCertificate;

		public override HttpClientCertificate ClientCertificate
		{
			get { return _clientCertificate; }
		}

		public void SetClientCertificate(HttpClientCertificate val)
		{
			_clientCertificate = val;
		}

		public override Encoding ContentEncoding { get; set; }

		private Int32 _contentLength;

		public override Int32 ContentLength
		{
			get { return _contentLength; }
		}

		public void SetContentLength(Int32 val)
		{
			_contentLength = val;
		}

		public override String ContentType { get; set; }

		private HttpCookieCollection _cookies = new HttpCookieCollection();

		public override HttpCookieCollection Cookies
		{
			get { return _cookies; }
		}

		public void SetCookies(HttpCookieCollection val)
		{
			_cookies = val;
		}

		private String _currentExecutionFilePath = "";

		public override String CurrentExecutionFilePath
		{
			get { return _currentExecutionFilePath; }
		}

		public void SetCurrentExecutionFilePath(String val)
		{
			_currentExecutionFilePath = val;
		}

		private String _currentExecutionFilePathExtension = "";

		public override String CurrentExecutionFilePathExtension
		{
			get { return _currentExecutionFilePathExtension; }
		}

		public void SetCurrentExecutionFilePathExtension(String val)
		{
			_currentExecutionFilePathExtension = val;
		}

		private String _filePath = "";

		public override String FilePath
		{
			get { return _filePath; }
		}

		public void SetFilePath(String val)
		{
			_filePath = val;
		}

		private HttpFileCollectionBase _files;

		public override HttpFileCollectionBase Files
		{
			get { return _files; }
		}

		public void SetFiles(HttpFileCollectionBase val)
		{
			_files = val;
		}

		public override Stream Filter { get; set; }

		private NameValueCollection _form = new NameValueCollection();

		public override NameValueCollection Form
		{
			get { return _form; }
		}

		public void SetForm(NameValueCollection val)
		{
			_form = val;
		}

		private String _httpMethod = "";

		public override String HttpMethod
		{
			get { return _httpMethod; }
		}

		public void SetHttpMethod(String val)
		{
			_httpMethod = val;
		}

		private Stream _inputStream = new MemoryStream();

		public override Stream InputStream
		{
			get { return _inputStream; }
		}

		public void SetInputStream(Stream val)
		{
			_inputStream = val;
		}

		private Boolean _isAuthenticated;

		public override Boolean IsAuthenticated
		{
			get { return _isAuthenticated; }
		}

		public void SetIsAuthenticated(Boolean val)
		{
			_isAuthenticated = val;
		}

		private Boolean _isLocal;

		public override Boolean IsLocal
		{
			get { return _isLocal; }
		}

		public void SetIsLocal(Boolean val)
		{
			_isLocal = val;
		}

		private Boolean _isSecureConnection;

		public override Boolean IsSecureConnection
		{
			get { return _isSecureConnection; }
		}

		public void SetIsSecureConnection(Boolean val)
		{
			_isSecureConnection = val;
		}

		private WindowsIdentity _logonUserIdentity;

		public override WindowsIdentity LogonUserIdentity
		{
			get { return _logonUserIdentity; }
		}

		public void SetLogonUserIdentity(WindowsIdentity val)
		{
			_logonUserIdentity = val;
		}

		private NameValueCollection _params = new NameValueCollection();

		public override NameValueCollection Params
		{
			get { return _params; }
		}

		public void SetParams(NameValueCollection val)
		{
			_params = val;
		}

		private String _path = "";

		public override String Path
		{
			get { return _path; }
		}

		public void SetPath(String val)
		{
			_path = val;
		}

		private String _pathInfo = "";

		public override String PathInfo
		{
			get { return _pathInfo; }
		}

		public void SetPathInfo(String val)
		{
			_pathInfo = val;
		}

		private String _physicalApplicationPath = "";

		public override String PhysicalApplicationPath
		{
			get { return _physicalApplicationPath; }
		}

		public void SetPhysicalApplicationPath(String val)
		{
			_physicalApplicationPath = val;
		}

		private String _physicalPath = "";

		public override String PhysicalPath
		{
			get { return _physicalPath; }
		}

		public void SetPhysicalPath(String val)
		{
			_physicalPath = val;
		}

		private String _rawUrl = "";

		public override String RawUrl
		{
			get { return _rawUrl; }
		}

		public void SetRawUrl(String val)
		{
			_rawUrl = val;
		}

		private ReadEntityBodyMode _readEntityBodyMode;

		public override ReadEntityBodyMode ReadEntityBodyMode
		{
			get { return _readEntityBodyMode; }
		}

		public void SetReadEntityBodyMode(ReadEntityBodyMode val)
		{
			_readEntityBodyMode = val;
		}

		public override RequestContext RequestContext { get; set; }

		public override String RequestType { get; set; }

		private NameValueCollection _serverVariables = new NameValueCollection();

		public override NameValueCollection ServerVariables
		{
			get { return _serverVariables; }
		}

		public void SetServerVariables(NameValueCollection val)
		{
			_serverVariables = val;
		}

		private CancellationToken _timedOutToken;

		public override CancellationToken TimedOutToken
		{
			get { return _timedOutToken; }
		}

		public void SetTimedOutToken(CancellationToken val)
		{
			_timedOutToken = val;
		}

		private Int32 _totalBytes;

		public override Int32 TotalBytes
		{
			get { return _totalBytes; }
		}

		public void SetTotalBytes(Int32 val)
		{
			_totalBytes = val;
		}

		private UnvalidatedRequestValuesBase _unvalidated;

		public override UnvalidatedRequestValuesBase Unvalidated
		{
			get { return _unvalidated; }
		}

		public void SetUnvalidated(UnvalidatedRequestValuesBase val)
		{
			_unvalidated = val;
		}

		private Uri _url;

		public override Uri Url
		{
			get { return _url; }
		}

		public void SetUrl(Uri val)
		{
			_url = val;
		}

		private Uri _urlReferrer;

		public override Uri UrlReferrer
		{
			get { return _urlReferrer; }
		}

		public void SetUrlReferrer(Uri val)
		{
			_urlReferrer = val;
		}

		private String _userAgent = "";

		public override String UserAgent
		{
			get { return _userAgent; }
		}

		public void SetUserAgent(String val)
		{
			_userAgent = val;
		}

		private String[] _userLanguages = { };

		public override String[] UserLanguages
		{
			get { return _userLanguages; }
		}

		public void SetUserLanguages(String[] val)
		{
			_userLanguages = val;
		}

		private String _userHostAddress = "";

		public override String UserHostAddress
		{
			get { return _userHostAddress; }
		}

		public void SetUserHostAddress(String val)
		{
			_userHostAddress = val;
		}

		private String _userHostName = "";

		public override String UserHostName
		{
			get { return _userHostName; }
		}

		public void SetUserHostName(String val)
		{
			_userHostName = val;
		}

		private NameValueCollection _headers = new NameValueCollection();

		public override NameValueCollection Headers
		{
			get { return _headers; }
		}

		public void SetHeaders(NameValueCollection val)
		{
			_headers = val;
		}

		private NameValueCollection _queryString = new NameValueCollection();

		public override NameValueCollection QueryString
		{
			get { return _queryString; }
		}

		public void SetQueryString(NameValueCollection val)
		{
			_queryString = val;
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

		public void ForceParamsLoading()
		{

			_params = new NameValueCollection();
			if (_form != null)
			{
				foreach (var item in _form.AllKeys)
				{
					_params.Add(item, _form[item]);
				}
			}
			if (_queryString != null)
			{
				foreach (var item in _queryString.AllKeys)
				{
					_params.Add(item, _queryString[item]);
				}
			}
		}

		public void ForceFileLoading()
		{
			_form = new NameValueCollection();

			if (ContentType == null)
			{
				_files = new FileCollection(new Dictionary<string, PostedFile>());
				return;
			}

			if (ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
			{
				var formUlrEncoded = new UrlEncodedStreamConverter();
				formUlrEncoded.Initialize(_inputStream, ContentEncoding, ContentType);
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
				multipartForm.Initialize(_inputStream, ContentEncoding, ContentType);
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
		}
	}
}