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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Web;
using ConcurrencyHelpers.Monitor;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Contexts
{
	public class NodeCsResponse : HttpResponseBase, IForcedHeadersResponse
	{
		const long TIMEOUT_MS = 60000;
		static NodeCsResponse()
		{
			var innerCollectionProperty = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.NonPublic | BindingFlags.Instance);
			_getInnerCollection = innerCollectionProperty.GetGetMethod(true);

		}
		private readonly HttpListenerResponse _response;
		private static readonly MethodInfo _getInnerCollection;
		private Timer _timeout;
		private Stopwatch _watch;

		private void Restart()
		{
			//_watch.Reset();
		}

		public NodeCsResponse(HttpListenerResponse response)
		{
			/*_watch = new Stopwatch();
			_watch.Start();
			_timeout = new Timer(TIMEOUT_MS);
			_timeout.Elapsed += OnTimeout;
			_timeout.Start();*/
			_response = response;
		}

		void OnTimeout(object sender, ElapsedEventArgs e)
		{
			if (_watch.ElapsedMilliseconds > TIMEOUT_MS)
			{
				_timeout.Dispose();
				try
				{
					Close();
				}
				catch
				{

				}
			}
		}

		public override Stream OutputStream
		{
			get
			{
				Restart();
				return _response.OutputStream;
			}
		}

		public override NameValueCollection Headers
		{
			get
			{
				Restart();
				return _response.Headers;
			}
		}

		public override string ContentType
		{
			get
			{
				Restart();
				return _response.ContentType;
			}
			set
			{
				_response.ContentType = value;
			}
		}

		public override Encoding ContentEncoding
		{
			get
			{
				Restart();
				return _response.ContentEncoding;
			}
			set
			{
				Restart();
				_response.ContentEncoding = value;
			}
		}

		public void ForceHeader(string key, string value)
		{
			Restart();
			var nameValueCollection = (NameValueCollection)_getInnerCollection.Invoke(_response.Headers, new object[] { });
			if (!_response.Headers.AllKeys.Contains(key))
			{
				nameValueCollection.Add(key, value);
			}
			else
			{
				nameValueCollection.Set(key, value);
			}
		}

		public override void SetCookie(HttpCookie cookie)
		{
			Restart();
			var discard = cookie.Expires.ToUniversalTime() < DateTime.UtcNow;
			_response.SetCookie(new Cookie(cookie.Name, cookie.Value)
			{
				Path = "/",
				Expires = discard ? DateTime.Now.AddDays(-1) : cookie.Expires,
				Secure = cookie.Secure
			});
		}

		public override void AppendCookie(HttpCookie cookie)
		{
			Restart();
			var discard = cookie.Expires.ToUniversalTime() < DateTime.UtcNow;
			_response.AppendCookie(new Cookie(cookie.Name, cookie.Value)
			{
				Path = "/",
				Expires = discard ? DateTime.Now.AddDays(-1) : cookie.Expires,
				Secure = cookie.Secure
			});
		}

		public override int StatusCode
		{
			get
			{
				return _response.StatusCode;
			}
			set
			{
				Restart();
				_response.StatusCode = value;
			}
		}

		private bool _closed = false;

		public override void Close()
		{
			if (!_closed)
			{
				PerfMon.SetMetric(PerfMonConst.NodeCs_Network_ClosedConnections, 1);
				_closed = true;
				try
				{
					_response.Close();
				}
				catch
				{
					Logger.Info("Connection already closed.");
				}
			}
		}

		public override void Redirect(string url)
		{
			Restart();
			_response.Redirect(url);
		}
	}
}
