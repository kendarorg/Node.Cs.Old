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


using System.Net;
using System;
using System.Collections;
using System.Threading.Tasks;
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
	public class ListenerHttpResponse : HttpResponseBase, IHttpResponse
	{
		public ListenerHttpResponse() { 
		
		}

		public object SourceObject { get { return _httpListenerResponse; } }

		private readonly HttpListenerResponse _httpListenerResponse;

		public ListenerHttpResponse(HttpListenerResponse httpListenerResponse)
		{
			_httpListenerResponse = httpListenerResponse;
			_httpListenerResponse.SendChunked = true;
			InitializeUnsettable();
		}

		public override void AddCacheItemDependency(String cacheKey)
		{
			//TODO Missing AddCacheItemDependency for HttpListenerResponse
		}

		public override void AddCacheItemDependencies(ArrayList cacheKeys)
		{
			//TODO Missing AddCacheItemDependencies for HttpListenerResponse
		}

		public override void AddCacheItemDependencies(String[] cacheKeys)
		{
			//TODO Missing AddCacheItemDependencies for HttpListenerResponse
		}

		public override void AddCacheDependency(params CacheDependency[] dependencies)
		{
			//TODO Missing AddCacheDependency for HttpListenerResponse
		}

		public override void AddFileDependency(String filename)
		{
			//TODO Missing AddFileDependency for HttpListenerResponse
		}

		public override void AddFileDependencies(ArrayList filenames)
		{
			//TODO Missing AddFileDependencies for HttpListenerResponse
		}

		public override void AddFileDependencies(String[] filenames)
		{
			//TODO Missing AddFileDependencies for HttpListenerResponse
		}

		public override void AddHeader(String name, String value)
		{
			_httpListenerResponse.AddHeader(name, value);
		}

		public override void AppendHeader(String name, String value)
		{
			_httpListenerResponse.AppendHeader(name, value);
		}

		public override void AppendToLog(String param)
		{
			//TODO Missing AppendToLog for HttpListenerResponse
		}

		public override String ApplyAppPathModifier(String virtualPath)
		{
			//TODO Missing ApplyAppPathModifier for HttpListenerResponse
			return null;
		}

		public override IAsyncResult BeginFlush(AsyncCallback callback, Object state)
		{
			//TODO Missing BeginFlush for HttpListenerResponse
			return null;
		}

		public override void BinaryWrite(Byte[] buffer)
		{
			_httpListenerResponse.OutputStream.Write(buffer, 0, buffer.Length);
		}

		public override void Clear()
		{
			//TODO Missing Clear for HttpListenerResponse
		}

		public override void ClearContent()
		{
			//TODO Missing ClearContent for HttpListenerResponse
		}

		public override void ClearHeaders()
		{
			_httpListenerResponse.Headers.Clear();
		}

		public override void Close()
		{
			_httpListenerResponse.Close();
			ContextsManager.OnClose();
		}

		public override void DisableKernelCache()
		{
			//TODO Missing DisableKernelCache for HttpListenerResponse
		}

		public override void DisableUserCache()
		{
			//TODO Missing DisableUserCache for HttpListenerResponse
		}

		public override void End()
		{
			//TODO Missing End for HttpListenerResponse
		}

		public override void EndFlush(IAsyncResult asyncResult)
		{
			//TODO Missing EndFlush for HttpListenerResponse
		}

		public override void Flush()
		{
			//TODO Missing Flush for HttpListenerResponse
		}

		public override void Pics(String value)
		{
			//TODO Missing Pics for HttpListenerResponse
		}

		public override void Redirect(String url)
		{
			_httpListenerResponse.Redirect(url);
		}

		public override void RedirectToRoute(Object routeValues)
		{
			//TODO Missing RedirectToRoute for HttpListenerResponse
		}

		public override void RedirectToRoute(String routeName)
		{
			//TODO Missing RedirectToRoute for HttpListenerResponse
		}

		public override void RedirectToRoute(RouteValueDictionary routeValues)
		{
			//TODO Missing RedirectToRoute for HttpListenerResponse
		}

		public override void RedirectToRoute(String routeName, Object routeValues)
		{
			//TODO Missing RedirectToRoute for HttpListenerResponse
		}

		public override void RedirectToRoute(String routeName, RouteValueDictionary routeValues)
		{
			//TODO Missing RedirectToRoute for HttpListenerResponse
		}

		public override void RedirectToRoutePermanent(Object routeValues)
		{
			//TODO Missing RedirectToRoutePermanent for HttpListenerResponse
		}

		public override void RedirectToRoutePermanent(String routeName)
		{
			//TODO Missing RedirectToRoutePermanent for HttpListenerResponse
		}

		public override void RedirectToRoutePermanent(RouteValueDictionary routeValues)
		{
			//TODO Missing RedirectToRoutePermanent for HttpListenerResponse
		}

		public override void RedirectToRoutePermanent(String routeName, Object routeValues)
		{
			//TODO Missing RedirectToRoutePermanent for HttpListenerResponse
		}

		public override void RedirectToRoutePermanent(String routeName, RouteValueDictionary routeValues)
		{
			//TODO Missing RedirectToRoutePermanent for HttpListenerResponse
		}

		public override void RedirectPermanent(String url)
		{
			//TODO Missing RedirectPermanent for HttpListenerResponse
		}

		public override void RedirectPermanent(String url, Boolean endResponse)
		{
			//TODO Missing RedirectPermanent for HttpListenerResponse
		}

		public override void RemoveOutputCacheItem(String path)
		{
			//TODO Missing RemoveOutputCacheItem for HttpListenerResponse
		}

		public override void RemoveOutputCacheItem(String path, String providerName)
		{
			//TODO Missing RemoveOutputCacheItem for HttpListenerResponse
		}

		public override void TransmitFile(String filename)
		{
			//TODO Missing TransmitFile for HttpListenerResponse
		}

		public override void TransmitFile(String filename, Int64 offset, Int64 length)
		{
			//TODO Missing TransmitFile for HttpListenerResponse
		}

		public override void Write(Char ch)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(new []{ch});
			_httpListenerResponse.OutputStream.Write(bytes,0,bytes.Length);
		}

		public override void Write(Char[] buffer, Int32 index, Int32 count)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(buffer);
			_httpListenerResponse.OutputStream.Write(bytes, index, count);
		}

		public override void Write(Object obj)
		{
			//TODO Missing Write for HttpListenerResponse
		}

		public override void Write(String s)
		{
			if (ContentEncoding == null) ContentEncoding = Encoding.UTF8;
			var bytes = ContentEncoding.GetBytes(s);
			_httpListenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
		}

		public override void WriteFile(String filename)
		{
		    var response = this;
		    var file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		    file.CopyToAsync(_httpListenerResponse.OutputStream)
		        .ContinueWith(a =>
		        {
		            response.Close();
		            file.Close();
		        });
		}

		public override void WriteFile(String filename, Boolean readIntoMemory)
		{
            //TODO Missing WriteFile for HttpListenerResponse
		}

		public override void WriteFile(String filename, Int64 offset, Int64 size)
		{
			//TODO Missing WriteFile for HttpListenerResponse
		}

		public override void WriteFile(IntPtr fileHandle, Int64 offset, Int64 size)
		{
			//TODO Missing WriteFile for HttpListenerResponse
		}

		public override void WriteSubstitution(HttpResponseSubstitutionCallback callback)
		{
			//TODO Missing WriteSubstitution for HttpListenerResponse
		}

		public override bool Buffer { get; set; }

		public override bool BufferOutput { get; set; }


		private HttpCachePolicyBase _cache;
		public override HttpCachePolicyBase Cache { get { return _cache; } }

		public void SetCache(HttpCachePolicyBase val)
		{
			_cache = val;
		}

		public override string CacheControl { get; set; }

		public override string Charset { get; set; }


		private CancellationToken _clientDisconnectedToken;
		public override CancellationToken ClientDisconnectedToken { get { return _clientDisconnectedToken; } }

		public void SetClientDisconnectedToken(CancellationToken val)
		{
			_clientDisconnectedToken = val;
		}

		public override Encoding ContentEncoding
		{
			set
			{
				_httpListenerResponse.ContentEncoding = value;
			}
			get
			{
				return _httpListenerResponse.ContentEncoding;
			}
		}

		public override String ContentType
		{
			set
			{
				_httpListenerResponse.ContentType = value;
			}
			get
			{
				return _httpListenerResponse.ContentType;
			}
		}

		public override int Expires { get; set; }

		public override DateTime ExpiresAbsolute { get; set; }

		public override Stream Filter { get; set; }


		public override NameValueCollection Headers { get { return _httpListenerResponse.Headers; } }

		public void SetHeaders(NameValueCollection val)
		{
		}

		public override Encoding HeaderEncoding { get; set; }


		private Boolean _isClientConnected;
		public override Boolean IsClientConnected { get { return _isClientConnected; } }

		public void SetIsClientConnected(Boolean val)
		{
			_isClientConnected = val;
		}


		private Boolean _isRequestBeingRedirected;
		public override Boolean IsRequestBeingRedirected { get { return _isRequestBeingRedirected; } }

		public void SetIsRequestBeingRedirected(Boolean val)
		{
			_isRequestBeingRedirected = val;
		}

		public override TextWriter Output { get; set; }


		public override Stream OutputStream { get { return _httpListenerResponse.OutputStream; } }

		public void SetOutputStream(Stream val)
		{
		}

		public override String RedirectLocation
		{
			set
			{
				_httpListenerResponse.RedirectLocation = value;
			}
			get
			{
				return _httpListenerResponse.RedirectLocation;
			}
		}

		public override string Status { get; set; }

		public override Int32 StatusCode
		{
			set
			{
				_httpListenerResponse.StatusCode = value;
			}
			get
			{
				return _httpListenerResponse.StatusCode;
			}
		}

		public override String StatusDescription
		{
			set
			{
				_httpListenerResponse.StatusDescription = value;
			}
			get
			{
				return _httpListenerResponse.StatusDescription;
			}
		}

		public override int SubStatusCode { get; set; }


		private Boolean _supportsAsyncFlush;
		public override Boolean SupportsAsyncFlush { get { return _supportsAsyncFlush; } }

		public void SetSupportsAsyncFlush(Boolean val)
		{
			_supportsAsyncFlush = val;
		}

		public override bool SuppressContent { get; set; }

		public override bool SuppressFormsAuthenticationRedirect { get; set; }

		public override bool TrySkipIisCustomErrors { get; set; }

		public void InitializeUnsettable()
		{
			_cookies = ConverCookies(_httpListenerResponse.Cookies);
			//_cache=_httpListenerResponse.Cache;
			//_clientDisconnectedToken=_httpListenerResponse.ClientDisconnectedToken;
			//_isClientConnected=_httpListenerResponse.IsClientConnected;
			//_isRequestBeingRedirected=_httpListenerResponse.IsRequestBeingRedirected;
			//_supportsAsyncFlush=_httpListenerResponse.SupportsAsyncFlush;
		}

		public override void AppendCookie(HttpCookie cookie)
		{
			var discard = cookie.Expires.ToUniversalTime() < DateTime.UtcNow;
			_httpListenerResponse.AppendCookie(new Cookie(cookie.Name, cookie.Value)
			{
				Path = "/",
				Expires = discard ? DateTime.Now.AddDays(-1) : cookie.Expires,
				Secure = cookie.Secure
			});
		}

		public override void Redirect(String url, Boolean endResponse)
		{
			_httpListenerResponse.Redirect(url);
		}

		public override void SetCookie(HttpCookie cookie)
		{
			var discard = cookie.Expires.ToUniversalTime() < DateTime.UtcNow;
			_httpListenerResponse.AppendCookie(new Cookie(cookie.Name, cookie.Value)
			{
				Path = "/",
				Expires = discard ? DateTime.Now.AddDays(-1) : cookie.Expires,
				Secure = cookie.Secure
			});
		}

		private HttpCookieCollection _cookies;

		public override HttpCookieCollection Cookies { get { return _cookies; } }

		public void SetCookies(HttpCookieCollection val)
		{
		}

		private HttpCookieCollection ConverCookies(CookieCollection cookies)
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


		public void Close(byte[] data, bool willblock = false)
		{
			_httpListenerResponse.Close(data,willblock);
		}
	}
}