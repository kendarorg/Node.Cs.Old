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
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Routing;

namespace Http.Shared.Contexts
{
	public interface IHttpRequest
	{
		void Abort();
		Byte[] BinaryRead(Int32 count);
		Stream GetBufferedInputStream();
		Stream GetBufferlessInputStream();
		Stream GetBufferlessInputStream(Boolean disableMaxRequestLength);
		void InsertEntityBody();
		void InsertEntityBody(Byte[] buffer, Int32 offset, Int32 count);
		Int32[] MapImageCoordinates(String imageFieldName);
		Double[] MapRawImageCoordinates(String imageFieldName);
		String MapPath(String virtualPath);
		String MapPath(String virtualPath, String baseVirtualDir, Boolean allowCrossAppMapping);
		void ValidateInput();
		void SaveAs(String filename, Boolean includeHeaders);
		String[] AcceptTypes { get; }
		String ApplicationPath { get; }
		String AnonymousID { get; }
		String AppRelativeCurrentExecutionFilePath { get; }
		HttpBrowserCapabilitiesBase Browser { get; }
		ChannelBinding HttpChannelBinding { get; }
		HttpClientCertificate ClientCertificate { get; }
		Encoding ContentEncoding { get; set; }
		Int32 ContentLength { get; }
		String ContentType { get; set; }
		HttpCookieCollection Cookies { get; }
		String CurrentExecutionFilePath { get; }
		String CurrentExecutionFilePathExtension { get; }
		String FilePath { get; }
		HttpFileCollectionBase Files { get; }
		Stream Filter { get; set; }
		NameValueCollection Form { get; }
		String HttpMethod { get; }
		Stream InputStream { get; }
		Boolean IsAuthenticated { get; }
		Boolean IsLocal { get; }
		Boolean IsSecureConnection { get; }
		WindowsIdentity LogonUserIdentity { get; }
		NameValueCollection Params { get; }
		String Path { get; }
		String PathInfo { get; }
		String PhysicalApplicationPath { get; }
		String PhysicalPath { get; }
		String RawUrl { get; }
		ReadEntityBodyMode ReadEntityBodyMode { get; }
		RequestContext RequestContext { get; set; }
		String RequestType { get; set; }
		NameValueCollection ServerVariables { get; }
		CancellationToken TimedOutToken { get; }
		Int32 TotalBytes { get; }
		UnvalidatedRequestValuesBase Unvalidated { get; }
		Uri Url { get; }
		Uri UrlReferrer { get; }
		String UserAgent { get; }
		String[] UserLanguages { get; }
		String UserHostAddress { get; }
		String UserHostName { get; }
		NameValueCollection Headers { get; }
		NameValueCollection QueryString { get; }
		Dictionary<string, string> ItemDictionary { get; }
		void SetAcceptTypes(String[] val);
		void SetApplicationPath(String val);
		void SetAnonymousID(String val);
		void SetAppRelativeCurrentExecutionFilePath(String val);
		void SetBrowser(HttpBrowserCapabilitiesBase val);
		void SetHttpChannelBinding(ChannelBinding val);
		void SetClientCertificate(HttpClientCertificate val);
		void SetContentLength(Int32 val);
		void SetCookies(HttpCookieCollection val);
		void SetCurrentExecutionFilePath(String val);
		void SetCurrentExecutionFilePathExtension(String val);
		void SetFilePath(String val);
		void SetFiles(HttpFileCollectionBase val);
		void SetForm(NameValueCollection val);
		void SetHttpMethod(String val);
		void SetInputStream(Stream val);
		void SetIsAuthenticated(Boolean val);
		void SetIsLocal(Boolean val);
		void SetIsSecureConnection(Boolean val);
		void SetLogonUserIdentity(WindowsIdentity val);
		void SetParams(NameValueCollection val);
		void SetPath(String val);
		void SetPathInfo(String val);
		void SetPhysicalApplicationPath(String val);
		void SetPhysicalPath(String val);
		void SetRawUrl(String val);
		void SetReadEntityBodyMode(ReadEntityBodyMode val);
		void SetServerVariables(NameValueCollection val);
		void SetTimedOutToken(CancellationToken val);
		void SetTotalBytes(Int32 val);
		void SetUnvalidated(UnvalidatedRequestValuesBase val);
		void SetUrl(Uri val);
		void SetUrlReferrer(Uri val);
		void SetUserAgent(String val);
		void SetUserLanguages(String[] val);
		void SetUserHostAddress(String val);
		void SetUserHostName(String val);
		void SetHeaders(NameValueCollection val);
		void SetQueryString(NameValueCollection val);
		string this[string key] { get; }
	}
}