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
using System.Collections;
using System.Web.Caching;
using System.Web;
using System.Web.Routing;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class SimpleHttpResponse : HttpResponseBase, IHttpResponse
	{
		public object SourceObject { get { return null; } }
		public override void AddCacheItemDependency(String cacheKey)
		{
		}
		
		public override void AddCacheItemDependencies(ArrayList cacheKeys)
		{
		}
		
		public override void AddCacheItemDependencies(String[] cacheKeys)
		{
		}
		
		public override void AddCacheDependency(params CacheDependency[] dependencies)
		{
		}
		
		public override void AddFileDependency(String filename)
		{
		}
		
		public override void AddFileDependencies(ArrayList filenames)
		{
		}
		
		public override void AddFileDependencies(String[] filenames)
		{
		}
		
		public override void AddHeader(String name, String value)
		{
			_headers.Add(name, value);
		}

		public void InitializeUnsettable()
		{
			
		}

		public override void AppendCookie(HttpCookie cookie)
		{
			_cookies.Add(cookie);
		}
		
		public override void AppendHeader(String name, String value)
		{
			_headers.Add(name, value);
		}
		
		public override void AppendToLog(String param)
		{
		}
		
		public override String ApplyAppPathModifier(String virtualPath)
		{
			return null;
		}
		
		public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
		{
			return null;
		}
		
		public override void BinaryWrite(Byte[] buffer)
		{
			_outputStream.Write(buffer, 0, buffer.Length);
		}
		
		public override void Clear()
		{
		}
		
		public override void ClearContent()
		{
		}
		
		public override void ClearHeaders()
		{
		}
		
		public override void Close()
		{
			if (_outputStream != null)
			{
				_outputStream.Close();
			}
		}
		
		public override void DisableKernelCache()
		{
		}
		
		public override void DisableUserCache()
		{
		}
		
		public override void End()
		{
		}
		
		public override void EndFlush(IAsyncResult asyncResult)
		{
		}
		
		public override void Flush()
		{
		}
		
		public override void Pics(String value)
		{
		}
		
		public override void Redirect(String url)
		{
		}
		
		public override void Redirect(String url, Boolean endResponse)
		{
		}
		
		public override void RedirectToRoute(Object routeValues)
		{
		}
		
		public override void RedirectToRoute(String routeName)
		{
		}
		
		public override void RedirectToRoute(RouteValueDictionary routeValues)
		{
		}
		
		public override void RedirectToRoute(String routeName, Object routeValues)
		{
		}
		
		public override void RedirectToRoute(String routeName, RouteValueDictionary routeValues)
		{
		}
		
		public override void RedirectToRoutePermanent(Object routeValues)
		{
		}
		
		public override void RedirectToRoutePermanent(String routeName)
		{
		}
		
		public override void RedirectToRoutePermanent(RouteValueDictionary routeValues)
		{
		}
		
		public override void RedirectToRoutePermanent(String routeName, Object routeValues)
		{
		}
		
		public override void RedirectToRoutePermanent(String routeName, RouteValueDictionary routeValues)
		{
		}
		
		public override void RedirectPermanent(String url)
		{
		}
		
		public override void RedirectPermanent(String url, Boolean endResponse)
		{
		}
		
		public override void RemoveOutputCacheItem(String path)
		{
		}
		
		public override void RemoveOutputCacheItem(String path, String providerName)
		{
		}
		
		public override void SetCookie(HttpCookie cookie)
		{
			_cookies.Set(cookie);
		}
		
		public override void TransmitFile(String filename)
		{
		}
		
		public override void TransmitFile(String filename, Int64 offset, Int64 length)
		{
		}
		
		public override void Write(Char ch)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(new[] { ch });
			_outputStream.Write(bytes, 0, bytes.Length);
		}
		
		public override void Write(Char[] buffer, Int32 index, Int32 count)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(buffer);
			_outputStream.Write(bytes, index, count);
		}
		
		public override void Write(Object obj)
		{
		}
		
		public override void Write(String s)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(s);
			_outputStream.Write(bytes, 0, bytes.Length);
		}
		
		public override void WriteFile(String filename)
		{
		}
		
		public override void WriteFile(String filename, Boolean readIntoMemory)
		{
		}
		
		public override void WriteFile(String filename, Int64 offset, Int64 size)
		{
		}
		
		public override void WriteFile(IntPtr fileHandle, Int64 offset, Int64 size)
		{
		}
		
		public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
		{
		}
		public override Boolean Buffer { get; set; }
		
		public override Boolean BufferOutput { get; set; }
		
		private HttpCachePolicyBase _cache;
		
		public override HttpCachePolicyBase Cache { get {return _cache; } }
		
		public void SetCache(HttpCachePolicyBase val)
		{
			_cache=val;
		}
		
		public override String CacheControl { get; set; }
		
		public override String Charset { get; set; }
		
		private CancellationToken _clientDisconnectedToken;
		
		public override CancellationToken ClientDisconnectedToken { get {return _clientDisconnectedToken; } }
		
		public void SetClientDisconnectedToken(CancellationToken val)
		{
			_clientDisconnectedToken=val;
		}
		
		public override Encoding ContentEncoding { get; set; }
		
		public override String ContentType { get; set; }
		
		private HttpCookieCollection _cookies= new HttpCookieCollection();
		
		public override HttpCookieCollection Cookies { get {return _cookies; } }
		
		public void SetCookies(HttpCookieCollection val)
		{
			_cookies=val;
		}
		
		public override Int32 Expires { get; set; }
		
		public override DateTime ExpiresAbsolute { get; set; }
		
		public override Stream Filter { get; set; }
		
		private NameValueCollection _headers= new NameValueCollection();
		
		public override NameValueCollection Headers { get {return _headers; } }
		
		public void SetHeaders(NameValueCollection val)
		{
			_headers=val;
		}
		
		public override Encoding HeaderEncoding { get; set; }
		
		private Boolean _isClientConnected;
		
		public override Boolean IsClientConnected { get {return _isClientConnected; } }
		
		public void SetIsClientConnected(Boolean val)
		{
			_isClientConnected=val;
		}
		
		private Boolean _isRequestBeingRedirected;
		
		public override Boolean IsRequestBeingRedirected { get {return _isRequestBeingRedirected; } }
		
		public void SetIsRequestBeingRedirected(Boolean val)
		{
			_isRequestBeingRedirected=val;
		}
		
		public override TextWriter Output { get; set; }
		
		private Stream _outputStream= new MemoryStream();
		
		public override Stream OutputStream { get {return _outputStream; } }
		
		public void SetOutputStream(Stream val)
		{
			_outputStream=val;
		}
		
		public override String RedirectLocation { get; set; }
		
		public override String Status { get; set; }
		
		public override Int32 StatusCode { get; set; }
		
		public override String StatusDescription { get; set; }
		
		public override Int32 SubStatusCode { get; set; }
		
		private Boolean _supportsAsyncFlush;
		
		public override Boolean SupportsAsyncFlush { get {return _supportsAsyncFlush; } }
		
		public void SetSupportsAsyncFlush(Boolean val)
		{
			_supportsAsyncFlush=val;
		}
		
		public override Boolean SuppressContent { get; set; }
		
		public override Boolean SuppressFormsAuthenticationRedirect { get; set; }
		
		public override Boolean TrySkipIisCustomErrors { get; set; }

		public void Close(byte[] data, bool willblock = true)
		{
			_outputStream.Write(data, 0, data.Length);
		}
	}
}