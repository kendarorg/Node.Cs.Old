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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Routing;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class FromBaseHttpResponse:HttpResponseBase,IHttpResponse
	{
		public FromBaseHttpResponse() {}

		public object SourceObject { get { return _httpResponseBase; } }
		
		private readonly HttpResponseBase _httpResponseBase;
		
		public FromBaseHttpResponse(HttpResponseBase httpResponseBase)
		{
			_httpResponseBase=httpResponseBase;
			InitializeUnsettable();
		}
		
		public override void AddCacheItemDependency(String cacheKey)
		{
			_httpResponseBase.AddCacheItemDependency(cacheKey);
		}
		
		public override void AddCacheItemDependencies(ArrayList cacheKeys)
		{
			_httpResponseBase.AddCacheItemDependencies(cacheKeys);
		}
		
		public override void AddCacheItemDependencies(String[] cacheKeys)
		{
			_httpResponseBase.AddCacheItemDependencies(cacheKeys);
		}
		
		public override void AddCacheDependency(params CacheDependency[] dependencies)
		{
			_httpResponseBase.AddCacheDependency(dependencies);
		}
		
		public override void AddFileDependency(String filename)
		{
			_httpResponseBase.AddFileDependency(filename);
		}
		
		public override void AddFileDependencies(ArrayList filenames)
		{
			_httpResponseBase.AddFileDependencies(filenames);
		}
		
		public override void AddFileDependencies(String[] filenames)
		{
			_httpResponseBase.AddFileDependencies(filenames);
		}
		
		public override void AddHeader(String name, String value)
		{
			_httpResponseBase.AddHeader(name, value);
		}
		
		public override void AppendCookie(HttpCookie cookie)
		{
			_httpResponseBase.AppendCookie(cookie);
		}
		
		public override void AppendHeader(String name, String value)
		{
			_httpResponseBase.AppendHeader(name, value);
		}
		
		public override void AppendToLog(String param)
		{
			_httpResponseBase.AppendToLog(param);
		}
		
		public override String ApplyAppPathModifier(String virtualPath)
		{
			return _httpResponseBase.ApplyAppPathModifier(virtualPath);
		}
		
		public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
		{
			return _httpResponseBase.BeginFlush(callback, state);
		}
		
		public override void BinaryWrite(Byte[] buffer)
		{
			_httpResponseBase.BinaryWrite(buffer);
		}
		
		public override void Clear()
		{
			_httpResponseBase.Clear();
		}
		
		public override void ClearContent()
		{
			_httpResponseBase.ClearContent();
		}
		
		public override void ClearHeaders()
		{
			_httpResponseBase.ClearHeaders();
		}
		
		public override void Close()
		{
			_httpResponseBase.Close();
			if (!(_httpResponseBase is SimpleHttpResponse)) ContextsManager.OnClose();
		}
		
		public override void DisableKernelCache()
		{
			_httpResponseBase.DisableKernelCache();
		}
		
		public override void DisableUserCache()
		{
			_httpResponseBase.DisableUserCache();
		}
		
		public override void End()
		{
			_httpResponseBase.End();
		}
		
		public override void EndFlush(IAsyncResult asyncResult)
		{
			_httpResponseBase.EndFlush(asyncResult);
		}
		
		public override void Flush()
		{
			_httpResponseBase.Flush();
		}
		
		public override void Pics(String value)
		{
			_httpResponseBase.Pics(value);
		}
		
		public override void Redirect(String url)
		{
			_httpResponseBase.Redirect(url);
		}
		
		public override void Redirect(String url, Boolean endResponse)
		{
			_httpResponseBase.Redirect(url, endResponse);
		}
		
		public override void RedirectToRoute(Object routeValues)
		{
			_httpResponseBase.RedirectToRoute(routeValues);
		}
		
		public override void RedirectToRoute(String routeName)
		{
			_httpResponseBase.RedirectToRoute(routeName);
		}
		
		public override void RedirectToRoute(RouteValueDictionary routeValues)
		{
			_httpResponseBase.RedirectToRoute(routeValues);
		}
		
		public override void RedirectToRoute(String routeName, Object routeValues)
		{
			_httpResponseBase.RedirectToRoute(routeName, routeValues);
		}
		
		public override void RedirectToRoute(String routeName, RouteValueDictionary routeValues)
		{
			_httpResponseBase.RedirectToRoute(routeName, routeValues);
		}
		
		public override void RedirectToRoutePermanent(Object routeValues)
		{
			_httpResponseBase.RedirectToRoutePermanent(routeValues);
		}
		
		public override void RedirectToRoutePermanent(String routeName)
		{
			_httpResponseBase.RedirectToRoutePermanent(routeName);
		}
		
		public override void RedirectToRoutePermanent(RouteValueDictionary routeValues)
		{
			_httpResponseBase.RedirectToRoutePermanent(routeValues);
		}
		
		public override void RedirectToRoutePermanent(String routeName, Object routeValues)
		{
			_httpResponseBase.RedirectToRoutePermanent(routeName, routeValues);
		}
		
		public override void RedirectToRoutePermanent(String routeName, RouteValueDictionary routeValues)
		{
			_httpResponseBase.RedirectToRoutePermanent(routeName, routeValues);
		}
		
		public override void RedirectPermanent(String url)
		{
			_httpResponseBase.RedirectPermanent(url);
		}
		
		public override void RedirectPermanent(String url, Boolean endResponse)
		{
			_httpResponseBase.RedirectPermanent(url, endResponse);
		}
		
		public override void RemoveOutputCacheItem(String path)
		{
			_httpResponseBase.RemoveOutputCacheItem(path);
		}
		
		public override void RemoveOutputCacheItem(String path, String providerName)
		{
			_httpResponseBase.RemoveOutputCacheItem(path, providerName);
		}
		
		public override void SetCookie(HttpCookie cookie)
		{
			_httpResponseBase.SetCookie(cookie);
		}
		
		public override void TransmitFile(String filename)
		{
			_httpResponseBase.TransmitFile(filename);
		}
		
		public override void TransmitFile(String filename, Int64 offset, Int64 length)
		{
			_httpResponseBase.TransmitFile(filename, offset, length);
		}
		
		public override void Write(Char ch)
		{
			_httpResponseBase.Write(ch);
		}
		
		public override void Write(Char[] buffer, Int32 index, Int32 count)
		{
			_httpResponseBase.Write(buffer, index, count);
		}
		
		public override void Write(Object obj)
		{
			_httpResponseBase.Write(obj);
		}
		
		public override void Write(String s)
		{
			_httpResponseBase.Write(s);
		}
		
		public override void WriteFile(String filename)
		{
			_httpResponseBase.WriteFile(filename);
		}
		
		public override void WriteFile(String filename, Boolean readIntoMemory)
		{
			_httpResponseBase.WriteFile(filename, readIntoMemory);
		}
		
		public override void WriteFile(String filename, Int64 offset, Int64 size)
		{
			_httpResponseBase.WriteFile(filename, offset, size);
		}
		
		public override void WriteFile(IntPtr fileHandle, Int64 offset, Int64 size)
		{
			_httpResponseBase.WriteFile(fileHandle, offset, size);
		}
		
		public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
		{
			_httpResponseBase.WriteSubstitution(callback);
		}
		
		public override Boolean Buffer
		{
			set
			{
				_httpResponseBase.Buffer=value;
			}
			get
			{
				return _httpResponseBase.Buffer;
			}
		}
		
		public override Boolean BufferOutput
		{
			set
			{
				_httpResponseBase.BufferOutput=value;
			}
			get
			{
				return _httpResponseBase.BufferOutput;
			}
		}
		
		
		public override HttpCachePolicyBase Cache { get {return _httpResponseBase.Cache; } }
		
		public void SetCache(HttpCachePolicyBase val)
		{
		}
		
		public override String CacheControl
		{
			set
			{
				_httpResponseBase.CacheControl=value;
			}
			get
			{
				return _httpResponseBase.CacheControl;
			}
		}
		
		public override String Charset
		{
			set
			{
				_httpResponseBase.Charset=value;
			}
			get
			{
				return _httpResponseBase.Charset;
			}
		}
		
		
		public override CancellationToken ClientDisconnectedToken { get {return _httpResponseBase.ClientDisconnectedToken; } }
		
		public void SetClientDisconnectedToken(CancellationToken val)
		{
		}
		
		public override Encoding ContentEncoding
		{
			set
			{
				_httpResponseBase.ContentEncoding=value;
			}
			get
			{
				return _httpResponseBase.ContentEncoding;
			}
		}
		
		public override String ContentType
		{
			set
			{
				_httpResponseBase.ContentType=value;
			}
			get
			{
				return _httpResponseBase.ContentType;
			}
		}
		
		
		public override HttpCookieCollection Cookies { get {return _httpResponseBase.Cookies; } }
		
		public void SetCookies(HttpCookieCollection val)
		{
		}
		
		public override Int32 Expires
		{
			set
			{
				_httpResponseBase.Expires=value;
			}
			get
			{
				return _httpResponseBase.Expires;
			}
		}
		
		public override DateTime ExpiresAbsolute
		{
			set
			{
				_httpResponseBase.ExpiresAbsolute=value;
			}
			get
			{
				return _httpResponseBase.ExpiresAbsolute;
			}
		}
		
		public override Stream Filter
		{
			set
			{
				_httpResponseBase.Filter=value;
			}
			get
			{
				return _httpResponseBase.Filter;
			}
		}
		
		
		public override NameValueCollection Headers { get {return _httpResponseBase.Headers; } }
		
		public void SetHeaders(NameValueCollection val)
		{
		}
		
		public override Encoding HeaderEncoding
		{
			set
			{
				_httpResponseBase.HeaderEncoding=value;
			}
			get
			{
				return _httpResponseBase.HeaderEncoding;
			}
		}
		
		
		public override Boolean IsClientConnected { get {return _httpResponseBase.IsClientConnected; } }
		
		public void SetIsClientConnected(Boolean val)
		{
		}
		
		
		public override Boolean IsRequestBeingRedirected { get {return _httpResponseBase.IsRequestBeingRedirected; } }
		
		public void SetIsRequestBeingRedirected(Boolean val)
		{
		}
		
		public override TextWriter Output
		{
			set
			{
				_httpResponseBase.Output=value;
			}
			get
			{
				return _httpResponseBase.Output;
			}
		}
		
		
		public override Stream OutputStream { get {return _httpResponseBase.OutputStream; } }
		
		public void SetOutputStream(Stream val)
		{
		}
		
		public override String RedirectLocation
		{
			set
			{
				_httpResponseBase.RedirectLocation=value;
			}
			get
			{
				return _httpResponseBase.RedirectLocation;
			}
		}
		
		public override String Status
		{
			set
			{
				_httpResponseBase.Status=value;
			}
			get
			{
				return _httpResponseBase.Status;
			}
		}
		
		public override Int32 StatusCode
		{
			set
			{
				_httpResponseBase.StatusCode=value;
			}
			get
			{
				return _httpResponseBase.StatusCode;
			}
		}
		
		public override String StatusDescription
		{
			set
			{
				_httpResponseBase.StatusDescription=value;
			}
			get
			{
				return _httpResponseBase.StatusDescription;
			}
		}
		
		public override Int32 SubStatusCode
		{
			set
			{
				_httpResponseBase.SubStatusCode=value;
			}
			get
			{
				return _httpResponseBase.SubStatusCode;
			}
		}
		
		
		public override Boolean SupportsAsyncFlush { get {return _httpResponseBase.SupportsAsyncFlush; } }
		
		public void SetSupportsAsyncFlush(Boolean val)
		{
		}
		
		public override Boolean SuppressContent
		{
			set
			{
				_httpResponseBase.SuppressContent=value;
			}
			get
			{
				return _httpResponseBase.SuppressContent;
			}
		}
		
		public override Boolean SuppressFormsAuthenticationRedirect
		{
			set
			{
				_httpResponseBase.SuppressFormsAuthenticationRedirect=value;
			}
			get
			{
				return _httpResponseBase.SuppressFormsAuthenticationRedirect;
			}
		}
		
		public override Boolean TrySkipIisCustomErrors
		{
			set
			{
				_httpResponseBase.TrySkipIisCustomErrors=value;
			}
			get
			{
				return _httpResponseBase.TrySkipIisCustomErrors;
			}
		}
		public void InitializeUnsettable()
		{
		}

		public void Close(byte[] data, bool willblock = true)
		{
			if (!willblock) _httpResponseBase.OutputStream.WriteAsync(data, 0, data.Length).ContinueWith(a => Close());
			else { _httpResponseBase.OutputStream.Write(data, 0, data.Length); Close(); }
		}
	}
}