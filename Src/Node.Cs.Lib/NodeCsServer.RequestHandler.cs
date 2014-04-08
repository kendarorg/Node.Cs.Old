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


using ConcurrencyHelpers.Coroutines;
using ConcurrencyHelpers.Monitor;
using NetworkHelpers;
using NetworkHelpers.Coroutines;
using NetworkHelpers.Http;
using Node.Cs.Lib.Utils;
using System;
using System.Threading;

namespace Node.Cs.Lib
{
	public partial class NodeCsServer
	{
		private void InitializeHttpListener()
		{
			var acceptor = new HttpCoroutineAcceptor();
			_server = new CoroutineNetwork(() =>
			{
				var httpListener = new HttpCoroutineListener();
				httpListener.OnReceived += OnHttpListenerReceived;
				return httpListener;
			}, acceptor, GlobalVars.Settings.Listener.GetPrefix())
			{
				MaxConcurrentlyRunningClients = MaxExecutingReqeuest,
				MaxConcurrentConnections = MaxConcurrentConnections
			};
			
			_server.Start();
		}

		private void OnHttpListenerReceived(object sender, IncomingDataReceivedEventArgs e)
		{
			PerfMon.SetMetric(PerfMonConst.NodeCs_Network_OpenedConnections, 1);
			var listener = sender as HttpCoroutineListenerClient;
			if (listener == null) return;
			var onReceived = new OnHttpListenerReceivedCoroutine();
			onReceived.Initialize(this, listener);
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
