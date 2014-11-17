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


using System.Web;
using System;
using System.Collections;
using System.Web.Caching;
using System.Web.Routing;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Http.Shared.Contexts;

namespace Http.Contexts
{
	public class WrappedHttpResponse : HttpResponseBase, IHttpResponse
	{
		public WrappedHttpResponse() { }
		public object SourceObject { get { return _httpResponse; } }
		private readonly IHttpResponse _httpResponse;

		public WrappedHttpResponse(IHttpResponse httpResponse)
		{
			_httpResponse = httpResponse;
			InitializeUnsettable();
			SetOutputStream(new MemoryStream());
		}

		public override void AddCacheItemDependency(String cacheKey)
		{
			_httpResponse.AddCacheItemDependency(cacheKey);
		}

		public override void AddCacheItemDependencies(ArrayList cacheKeys)
		{
			_httpResponse.AddCacheItemDependencies(cacheKeys);
		}

		public override void AddCacheItemDependencies(String[] cacheKeys)
		{
			_httpResponse.AddCacheItemDependencies(cacheKeys);
		}

		public override void AddCacheDependency(params CacheDependency[] dependencies)
		{
			_httpResponse.AddCacheDependency(dependencies);
		}

		public override void AddFileDependency(String filename)
		{
			_httpResponse.AddFileDependency(filename);
		}

		public override void AddFileDependencies(ArrayList filenames)
		{
			_httpResponse.AddFileDependencies(filenames);
		}

		public override void AddFileDependencies(String[] filenames)
		{
			_httpResponse.AddFileDependencies(filenames);
		}

		public override void AddHeader(String name, String value)
		{
			_httpResponse.AddHeader(name, value);
		}

		public override void AppendCookie(HttpCookie cookie)
		{
			_httpResponse.AppendCookie(cookie);
		}

		public override void AppendHeader(String name, String value)
		{
			_httpResponse.AppendHeader(name, value);
		}

		public override void AppendToLog(String param)
		{
			_httpResponse.AppendToLog(param);
		}

		public override String ApplyAppPathModifier(String virtualPath)
		{
			return _httpResponse.ApplyAppPathModifier(virtualPath);
		}

		public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
		{
			return _httpResponse.BeginFlush(callback, state);
		}

		public override void BinaryWrite(Byte[] buffer)
		{
			_httpResponse.BinaryWrite(buffer);
		}

		public override void Clear()
		{
			_httpResponse.Clear();
		}

		public override void ClearContent()
		{
			_httpResponse.ClearContent();
		}

		public override void ClearHeaders()
		{
			_httpResponse.ClearHeaders();
		}

		public override void Close()
		{
			_httpResponse.Close();
			ContextsManager.OnClose();
		}

		public override void DisableKernelCache()
		{
			_httpResponse.DisableKernelCache();
		}

		public override void DisableUserCache()
		{
			_httpResponse.DisableUserCache();
		}

		public override void End()
		{
			_httpResponse.End();
		}

		public override void EndFlush(IAsyncResult asyncResult)
		{
			_httpResponse.EndFlush(asyncResult);
		}

		public override void Flush()
		{
			_httpResponse.Flush();
		}

		public override void Pics(String value)
		{
			_httpResponse.Pics(value);
		}

		public override void Redirect(String url)
		{
			_httpResponse.Redirect(url);
		}

		public override void Redirect(String url, Boolean endResponse)
		{
			_httpResponse.Redirect(url, endResponse);
		}

		public override void RedirectToRoute(Object routeValues)
		{
			_httpResponse.RedirectToRoute(routeValues);
		}

		public override void RedirectToRoute(String routeName)
		{
			_httpResponse.RedirectToRoute(routeName);
		}

		public override void RedirectToRoute(RouteValueDictionary routeValues)
		{
			_httpResponse.RedirectToRoute(routeValues);
		}

		public override void RedirectToRoute(String routeName, Object routeValues)
		{
			_httpResponse.RedirectToRoute(routeName, routeValues);
		}

		public override void RedirectToRoute(String routeName, RouteValueDictionary routeValues)
		{
			_httpResponse.RedirectToRoute(routeName, routeValues);
		}

		public override void RedirectToRoutePermanent(Object routeValues)
		{
			_httpResponse.RedirectToRoutePermanent(routeValues);
		}

		public override void RedirectToRoutePermanent(String routeName)
		{
			_httpResponse.RedirectToRoutePermanent(routeName);
		}

		public override void RedirectToRoutePermanent(RouteValueDictionary routeValues)
		{
			_httpResponse.RedirectToRoutePermanent(routeValues);
		}

		public override void RedirectToRoutePermanent(String routeName, Object routeValues)
		{
			_httpResponse.RedirectToRoutePermanent(routeName, routeValues);
		}

		public override void RedirectToRoutePermanent(String routeName, RouteValueDictionary routeValues)
		{
			_httpResponse.RedirectToRoutePermanent(routeName, routeValues);
		}

		public override void RedirectPermanent(String url)
		{
			_httpResponse.RedirectPermanent(url);
		}

		public override void RedirectPermanent(String url, Boolean endResponse)
		{
			_httpResponse.RedirectPermanent(url, endResponse);
		}

		public override void RemoveOutputCacheItem(String path)
		{
			//TODO Missing RemoveOutputCacheItem for HttpResponse
		}

		public override void RemoveOutputCacheItem(String path, String providerName)
		{
			//TODO Missing RemoveOutputCacheItem for HttpResponse
		}

		public override void SetCookie(HttpCookie cookie)
		{
			_httpResponse.SetCookie(cookie);
		}

		public override void TransmitFile(String filename)
		{
			_httpResponse.TransmitFile(filename);
		}

		public override void TransmitFile(String filename, Int64 offset, Int64 length)
		{
			_httpResponse.TransmitFile(filename, offset, length);
		}

		public override void Write(Char ch)
		{
			_httpResponse.Write(ch);
		}

		public override void Write(Char[] buffer, Int32 index, Int32 count)
		{
			_httpResponse.Write(buffer, index, count);
		}

		public override void Write(Object obj)
		{
			_httpResponse.Write(obj);
		}

		public override void Write(String s)
		{
			_httpResponse.Write(s);
		}

		public override void WriteFile(String filename)
		{
			_httpResponse.WriteFile(filename);
		}

		public override void WriteFile(String filename, Boolean readIntoMemory)
		{
			_httpResponse.WriteFile(filename, readIntoMemory);
		}

		public override void WriteFile(String filename, Int64 offset, Int64 size)
		{
			_httpResponse.WriteFile(filename, offset, size);
		}

		public override void WriteFile(IntPtr fileHandle, Int64 offset, Int64 size)
		{
			_httpResponse.WriteFile(fileHandle, offset, size);
		}

		public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
		{
			_httpResponse.WriteSubstitution(callback);
		}
		public override Boolean Buffer
		{
			set
			{
				_httpResponse.Buffer = value;
			}
			get
			{
				return _httpResponse.Buffer;
			}
		}

		public override Boolean BufferOutput
		{
			set
			{
				_httpResponse.BufferOutput = value;
			}
			get
			{
				return _httpResponse.BufferOutput;
			}
		}

		public override String CacheControl
		{
			set
			{
				_httpResponse.CacheControl = value;
			}
			get
			{
				return _httpResponse.CacheControl;
			}
		}

		public override String Charset
		{
			set
			{
				_httpResponse.Charset = value;
			}
			get
			{
				return _httpResponse.Charset;
			}
		}


		public override CancellationToken ClientDisconnectedToken { get { return _httpResponse.ClientDisconnectedToken; } }


		public void SetClientDisconnectedToken(CancellationToken val)
		{
		}

		public override Encoding ContentEncoding
		{
			set
			{
				_httpResponse.ContentEncoding = value;
			}
			get
			{
				return _httpResponse.ContentEncoding;
			}
		}

		public override String ContentType
		{
			set
			{
				_httpResponse.ContentType = value;
			}
			get
			{
				return _httpResponse.ContentType;
			}
		}


		public override HttpCookieCollection Cookies { get { return _httpResponse.Cookies; } }

		public void SetCookies(HttpCookieCollection val)
		{
		}

		public override Int32 Expires
		{
			set
			{
				_httpResponse.Expires = value;
			}
			get
			{
				return _httpResponse.Expires;
			}
		}

		public override DateTime ExpiresAbsolute
		{
			set
			{
				_httpResponse.ExpiresAbsolute = value;
			}
			get
			{
				return _httpResponse.ExpiresAbsolute;
			}
		}

		public override Stream Filter
		{
			set
			{
				_httpResponse.Filter = value;
			}
			get
			{
				return _httpResponse.Filter;
			}
		}


		public override NameValueCollection Headers { get { return _httpResponse.Headers; } }

		public void SetHeaders(NameValueCollection val)
		{
		}

		public override Encoding HeaderEncoding
		{
			set
			{
				_httpResponse.HeaderEncoding = value;
			}
			get
			{
				return _httpResponse.HeaderEncoding;
			}
		}


		public override Boolean IsClientConnected { get { return _httpResponse.IsClientConnected; } }

		public void SetIsClientConnected(Boolean val)
		{
		}


		public override Boolean IsRequestBeingRedirected { get { return _httpResponse.IsRequestBeingRedirected; } }

		public void SetIsRequestBeingRedirected(Boolean val)
		{
		}

		public override TextWriter Output
		{
			set
			{
				_httpResponse.Output = value;
			}
			get
			{
				return _httpResponse.Output;
			}
		}


		public override Stream OutputStream { get { return _outputStream; } }

		public void SetOutputStream(Stream val)
		{
			_outputStream = val;
		}

		public override String RedirectLocation
		{
			set
			{
				_httpResponse.RedirectLocation = value;
			}
			get
			{
				return _httpResponse.RedirectLocation;
			}
		}

		public override String Status
		{
			set
			{
				_httpResponse.Status = value;
			}
			get
			{
				return _httpResponse.Status;
			}
		}

		public override Int32 StatusCode
		{
			set
			{
				_httpResponse.StatusCode = value;
			}
			get
			{
				return _httpResponse.StatusCode;
			}
		}

		public override String StatusDescription
		{
			set
			{
				_httpResponse.StatusDescription = value;
			}
			get
			{
				return _httpResponse.StatusDescription;
			}
		}

		public override Int32 SubStatusCode
		{
			set
			{
				_httpResponse.SubStatusCode = value;
			}
			get
			{
				return _httpResponse.SubStatusCode;
			}
		}


		public override Boolean SupportsAsyncFlush { get { return _httpResponse.SupportsAsyncFlush; } }

		public void SetSupportsAsyncFlush(Boolean val)
		{
		}

		public override Boolean SuppressContent
		{
			set
			{
				_httpResponse.SuppressContent = value;
			}
			get
			{
				return _httpResponse.SuppressContent;
			}
		}

		public override Boolean SuppressFormsAuthenticationRedirect
		{
			set
			{
				_httpResponse.SuppressFormsAuthenticationRedirect = value;
			}
			get
			{
				return _httpResponse.SuppressFormsAuthenticationRedirect;
			}
		}

		public override Boolean TrySkipIisCustomErrors
		{
			set
			{
				_httpResponse.TrySkipIisCustomErrors = value;
			}
			get
			{
				return _httpResponse.TrySkipIisCustomErrors;
			}
		}
		public void InitializeUnsettable()
		{
			_cache = _httpResponse.Cache;
		}

		// ReSharper disable once UnusedParameter.Local
		private HttpCachePolicyBase ConvertCache(HttpCachePolicy cache)
		{
			return null;
		}

		private HttpCachePolicyBase _cache;
		private Stream _outputStream;
		public override HttpCachePolicyBase Cache { get { return _cache; } }

		public void SetCache(HttpCachePolicyBase val)
		{
		}

		public void Close(byte[] data, bool willblock = true)
		{
			if (!willblock) _httpResponse.OutputStream.WriteAsync(data, 0, data.Length).ContinueWith(a => Close());
			else { _httpResponse.OutputStream.Write(data, 0, data.Length); Close(); }
		}
	}
}