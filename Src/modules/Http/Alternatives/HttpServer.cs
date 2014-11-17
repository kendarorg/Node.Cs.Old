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


using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Http.Alternatives
{
	public abstract class HttpServer
	{
		private readonly IPAddress _address;
		protected int _port;
		TcpListener _listener;
		bool _is_active = true;

		protected HttpServer(IPAddress address, int port)
		{
			_address = address;
			_port = port;
		}

		public void Listen()
		{
			_listener = new TcpListener(_address, _port);
			_listener.Start();
			while (_is_active)
			{
				_listener.AcceptTcpClientAsync()
					.ContinueWith(a =>
					{
						var processor = new HttpProcessor(a.Result, this);
						processor.Process();
					});
			}
		}

		public abstract void HandleGetRequest(HttpProcessor p);
		public abstract void HandlePostRequest(HttpProcessor p, StreamReader inputData);
	}
}
