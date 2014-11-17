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


using System.Text;
using ConcurrencyHelpers.Coroutines;
using ConcurrencyHelpers.Monitor;
using NetworkHelpers.Commons;
using NetworkHelpers.Http;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.OnReceive;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib
{
	public partial class NodeCsServer
	{
		private void InitializeHttpListener()
		{
			//var acceptor = new HttpCoroutineAcceptor();
			_server = new CoroutineHttpNetwork(
				10000,
				new [] { GlobalVars.Settings.Listener.GetPrefix() },
				MaxConcurrentConnections);
			_server.Received += OnHttpListenerReceived;
			_server.Refuse += OnRefused;
			_server.StartServer();
		}

		private void OnRefused(object sender, RefuseEventArgs e)
		{
			
		}

		private static readonly byte[] _serverBusy = Encoding.UTF8.GetBytes("Server Busy.");
		private void OnHttpListenerReceived(object sender, ReceivedEventArgs e)
		{

			PerfMon.SetMetric(PerfMonConst.NodeCs_Network_OpenedConnections, 1);
			if (GlobalVars.OpenedConnections >= GlobalVars.Settings.Threading.MaxConcurrentConnections)
			{
				var rl = e.Request as ConnectionHttp;
				rl.Context.Response.StatusCode = 503;
				rl.Context.Response.Close(_serverBusy, false);
				return;
			}
			GlobalVars.OpenedConnections++;
			var listener = e.Request as ConnectionHttp;
			if (listener == null) return;
			var onReceived = new OnHttpListenerReceivedCoroutine();
			var listenerContainer = new ListenerContainer(listener);
			var pagesManager = new PagesManager();
			onReceived.Initialize(new ContextManager(listenerContainer), new SessionManager(), this, pagesManager);
			var chosenThread = _connectionsCount.Value % GlobalVars.Settings.Threading.ThreadNumber;
			_utilityThread[chosenThread].AddCoroutine(onReceived);
		}

		public CoroutineThread NextCoroutine
		{
			get
			{
				_connectionsCount++;
				var chosenThread = _connectionsCount.Value % GlobalVars.Settings.Threading.ThreadNumber;
				return _utilityThread[chosenThread];
			}
		}

	}
}
