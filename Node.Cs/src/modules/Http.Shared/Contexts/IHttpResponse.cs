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

namespace Http.Shared.Contexts
{
	public interface IHttpResponse
	{
		void AddCacheItemDependency(String cacheKey);
		void AddCacheItemDependencies(ArrayList cacheKeys);
		void AddCacheItemDependencies(String[] cacheKeys);
		void AddCacheDependency(params CacheDependency[] dependencies);
		void AddFileDependency(String filename);
		void AddFileDependencies(ArrayList filenames);
		void AddFileDependencies(String[] filenames);
		void AddHeader(String name, String value);
		void AppendHeader(String name, String value);
		void AppendToLog(String param);
		String ApplyAppPathModifier(String virtualPath);
		IAsyncResult BeginFlush(AsyncCallback callback, Object state);
		void BinaryWrite(Byte[] buffer);
		void Clear();
		void ClearContent();
		void ClearHeaders();
		void Close();
		void Close(byte[] data, bool willblock = false);
		void DisableKernelCache();
		void DisableUserCache();
		void End();
		void EndFlush(IAsyncResult asyncResult);
		void Flush();
		void Pics(String value);
		void Redirect(String url);
		void RedirectToRoute(Object routeValues);
		void RedirectToRoute(String routeName);
		void RedirectToRoute(RouteValueDictionary routeValues);
		void RedirectToRoute(String routeName, Object routeValues);
		void RedirectToRoute(String routeName, RouteValueDictionary routeValues);
		void RedirectToRoutePermanent(Object routeValues);
		void RedirectToRoutePermanent(String routeName);
		void RedirectToRoutePermanent(RouteValueDictionary routeValues);
		void RedirectToRoutePermanent(String routeName, Object routeValues);
		void RedirectToRoutePermanent(String routeName, RouteValueDictionary routeValues);
		void RedirectPermanent(String url);
		void RedirectPermanent(String url, Boolean endResponse);
		void RemoveOutputCacheItem(String path);
		void RemoveOutputCacheItem(String path, String providerName);
		void TransmitFile(String filename);
		void TransmitFile(String filename, Int64 offset, Int64 length);
		void Write(Char ch);
		void Write(Char[] buffer, Int32 index, Int32 count);
		void Write(Object obj);
		void Write(String s);
		void WriteFile(String filename);
		void WriteFile(String filename, Boolean readIntoMemory);
		void WriteFile(String filename, Int64 offset, Int64 size);
		void WriteFile(IntPtr fileHandle, Int64 offset, Int64 size);
		void WriteSubstitution(HttpResponseSubstitutionCallback callback);
		Boolean Buffer { set; get; }
		Boolean BufferOutput { set; get; }
		HttpCachePolicyBase Cache { get; }
		String CacheControl { set; get; }
		String Charset { set; get; }
		CancellationToken ClientDisconnectedToken { get; }
		Encoding ContentEncoding { set; get; }
		String ContentType { set; get; }
		Int32 Expires { set; get; }
		DateTime ExpiresAbsolute { set; get; }
		Stream Filter { set; get; }
		NameValueCollection Headers { get; }
		Encoding HeaderEncoding { set; get; }
		Boolean IsClientConnected { get; }
		Boolean IsRequestBeingRedirected { get; }
		TextWriter Output { set; get; }
		Stream OutputStream { get; }
		String RedirectLocation { set; get; }
		String Status { set; get; }
		Int32 StatusCode { set; get; }
		String StatusDescription { set; get; }
		Int32 SubStatusCode { set; get; }
		Boolean SupportsAsyncFlush { get; }
		Boolean SuppressContent { set; get; }
		Boolean SuppressFormsAuthenticationRedirect { set; get; }
		Boolean TrySkipIisCustomErrors { set; get; }
		HttpCookieCollection Cookies { get; }
		void SetCache(HttpCachePolicyBase val);
		void SetClientDisconnectedToken(CancellationToken val);
		void SetHeaders(NameValueCollection val);
		void SetIsClientConnected(Boolean val);
		void SetIsRequestBeingRedirected(Boolean val);
		void SetOutputStream(Stream val);
		void SetSupportsAsyncFlush(Boolean val);
		void InitializeUnsettable();
		void AppendCookie(HttpCookie cookie);
		void Redirect(String url, Boolean endResponse);
		void SetCookie(HttpCookie cookie);
		void SetCookies(HttpCookieCollection val);
	}
}