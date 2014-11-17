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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using ConcurrencyHelpers.Interfaces;
using ConcurrencyHelpers.Monitor;
using ConcurrencyHelpers.Utils;
using NetworkHelpers.Coroutines;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Conversions;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Loaders;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib
{
	public partial class NodeCsServer : INodeCsServer
	{
		private HandlersLoader _handlersLoader;

		public const String NodeCsCache = "Node.Cs.Lib.NodeCsServer";
		public const String SessionCache = "Node.Cs.Lib.NodeCsServer.SessionCache";

		public static string RootDir { get; private set; }
		private readonly CoroutineMemoryCache _memoryCache;
		private CoroutineNetwork _server;
		private CoroutineThread[] _utilityThread;

		public bool PrecompilePages { get; set; }
		public int MaxConcurrentConnections { get; set; }
		public int MaxExecutingReqeuest { get; set; }
		public bool IsRunning { get; set; }

		private CounterInt64 _connectionsCount;
		private ClassWrapper.ClassWrapper _globalInitializer;

		public static NodeCsServer Instance { get; private set; }

		public NodeCsServer(NodeCsSettings settings, string rootDir)
		{
			GlobalVars.NodeCsServer = this;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			Instance = this;
			GlobalVars.Settings = settings;

			GlobalVars.ResponseHandlers = new ResponseHandlersFactory();

			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			RootDir = rootDir;
			GlobalVars.Settings.ReRoot(rootDir);

			Thread.CurrentThread.CurrentCulture = GlobalVars.Settings.Listener.Cultures.DefaultCulture;

			foreach (var binpath in GlobalVars.Settings.Paths.BinPaths)
			{
				Environment.SetEnvironmentVariable("PATH",
				 Environment.GetEnvironmentVariable("PATH") + ";" + binpath);
			}

			InitializePerfMon();

			NodeCsSettings.Settings = settings;

			InitializeProviders(); LoadApplication(GlobalVars.Settings.Application, GlobalVars.Settings.Paths.BinPaths, rootDir);

			_connectionsCount = new CounterInt64();
			_memoryCache = new CoroutineMemoryCache(new TimeSpan(0, 0, 0, 60));

			GlobalVars.PathProvider = new GlobalPathProvider();
			GlobalVars.PathProvider.Initialize(null, null);

			PrecompilePages = false;
			MaxExecutingReqeuest = GlobalVars.Settings.Threading.MaxExecutingRequest;
			MaxConcurrentConnections = GlobalVars.Settings.Threading.MaxConcurrentConnections;
		}

		private SystemTimer _perfMonTimer;
		private long _totalTerminatedCoroutines;
		private long _totalStartedCoroutines;
		private PluginsLoader _pluginsInitializer;

		public void Start()
		{
			_memoryCache.CycleMaxMs = 5;
			_memoryCache.Start();
			GlobalVars.RoutingService = new RoutingService();
			//To reorder

			IsRunning = true;
			Console.WriteLine("Listening on port " + GlobalVars.Settings.Listener.Port);
			_memoryCache.CreateArea(NodeCsCache, new TimeSpan(0, 0, 1));
			_memoryCache.AddCoroutine(new CleanUpOnMaxMemoryReached(_memoryCache, NodeCsCache));

			_handlersLoader = new HandlersLoader(RootDir, _memoryCache);
			_pluginsInitializer = new PluginsLoader();

			InitializeLoggers();
			InitializeExceptionManager();



			GlobalVars.ExtensionHandler = new ExtensionHandler(_memoryCache, _handlersLoader);
			_utilityThread = new CoroutineThread[GlobalVars.Settings.Threading.ThreadNumber];
			var processorsCount = Environment.ProcessorCount;
			
			for (int i = 0; i < GlobalVars.Settings.Threading.ThreadNumber; i++)
			{
				var affinity = i%processorsCount;
				var realAffinity = 1<<affinity;
				_utilityThread[i] = new CoroutineThread(2);
				_utilityThread[i].Start();
			}
			GlobalVars.ExtensionHandler.InitializeHandlers();



			_pluginsInitializer.Initialize(RootDir);



			//Checked
			InitializeSessionStorage();
			InitializeConversionService();
			InvokeApplicationStart();
			InitializeControllersFactory();

			InitializeHttpListener();
		}

		public void Stop()
		{
			_server.Stop();
			for (int i = 0; i < GlobalVars.Settings.Threading.ThreadNumber; i++)
			{
				_utilityThread[i].Stop();
			}
			_memoryCache.Stop();
			IsRunning = false;
		}


		private void InitializeConversionService()
		{
			GlobalVars.ConversionService = new ConversionService();

			GlobalVars.ConversionService.AddConverter(new JsonNodeCsSerializer(),
				"application/json", "text/json", "text/javascript");

			GlobalVars.ConversionService.AddConverter(new XmlNodeCsSerializer(),
				"application/xml", "application/atom+xml", "text/xslt", "text/xml", "text/xsl");

			GlobalVars.ConversionService.AddConverter(new FormNodeCsSerializer(),
				"application/x-www-form-urlencoded", "multipart/form-data");
		}

		private void InitializeSessionStorage()
		{
			if (GlobalVars.SessionStorage == null)
			{
				GlobalVars.SessionStorage = new InMemorySessionStorage(_memoryCache, GlobalVars.Settings.Listener.SessionTimeout);
			}
		}

		private void InitializeExceptionManager()
		{
			if (_globalInitializer.ContainsMethod("InitializeExceptionManager"))
			{
				GlobalVars.ExceptionManager = _globalInitializer.InvokeReturn<IGlobalExceptionManager>("InitializeExceptionManager");
			}
			if (GlobalVars.ExceptionManager == null)
			{
				GlobalVars.ExceptionManager = new DefaultExceptionManager();
			}
			GlobalVars.ExceptionManager.Initialize(GlobalVars.Settings);
		}

		private void InvokeApplicationStart()
		{
			if (_globalInitializer.ContainsMethod("Application_Start"))
			{
				_globalInitializer.Invoke("Application_Start");
			}
		}

		private void InitializeLoggers()
		{
			if (_globalInitializer.ContainsMethod("InitializeLoggers"))
			{
				foreach (var logger in _globalInitializer.InvokeReturn<IEnumerable<ILogger>>("InitializeLoggers"))
				{
					logger.IsErrorEnabled = true;
					logger.IsWarningEnabled = true;
					Logger.RegisterLogger(logger);
				}
			}
			Logger.Submit();
		}

		private void InitializePerfMon()
		{
			const int runTimes = 10000;

			PerfMon.Start(runTimes);
			var dir = Path.Combine(RootDir, "PerfMon");
			Directory.CreateDirectory(dir);
			// ReSharper disable once UnusedVariable
			var pfServer = new FilePerfMonService(dir);


			PerfMon.AddMonitor(PerfMonConst.NodeCs_Cache_CacheItemsCount, new ValueCounterMetric(false));
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Threading_StartedCoroutines, new ValueCounterMetric());
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Threading_RunningCoroutines, new ValueCounterMetric(false));
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Threading_TerminatedCoroutines, new ValueCounterMetric());

			PerfMon.AddMonitor(PerfMonConst.NodeCs_Status_Status, new StatusMetric(true));
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Status_Exceptions, new ValueCounterMetric());

			PerfMon.AddMonitor(PerfMonConst.NodeCs_Network_OpenedConnections, new ValueCounterMetric());
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Network_CurrentConnections, new ValueCounterMetric(false));
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Network_ClosedConnections, new ValueCounterMetric());
			PerfMon.AddMonitor(PerfMonConst.NodeCs_Network_RequestDurations, new RunAndExcutionCounterMetric());

			_perfMonTimer = new SystemTimer(runTimes);
			_perfMonTimer.Elapsed += OnPerfMonElapsed;
			_perfMonTimer.Start();
		}

		void OnPerfMonElapsed(object sender, ElapsedTimerEventArgs e)
		{
			var data = PerfMon.Data;
			var opened = (ValueCounterMetric)
				data.FirstOrDefault((d) => string.Compare(d.Id, PerfMonConst.NodeCs_Network_OpenedConnections, StringComparison.OrdinalIgnoreCase) == 0);
			var closed = (ValueCounterMetric)
				data.FirstOrDefault(
					(d) => string.Compare(d.Id, PerfMonConst.NodeCs_Network_ClosedConnections, StringComparison.OrdinalIgnoreCase) == 0);
			if (closed != null && opened != null)
			{
				var running = opened.Value - closed.Value;
				PerfMon.SetMetric(PerfMonConst.NodeCs_Network_CurrentConnections, running);
				PerfMon.SetStatus(PerfMonConst.NodeCs_Status_Status, IsRunning);
			}

			if (_utilityThread != null)
			{
				long totalTerminatedCoroutines = 0;
				long totalStartedCoroutines = 0;
				long totalRunningCoroutines = 0;
				foreach (var th in _utilityThread)
				{
					if(th==null) continue;
					totalStartedCoroutines += th.StartedCoroutines;
					totalTerminatedCoroutines += th.TerminatedCoroutines;
					totalRunningCoroutines += (totalStartedCoroutines - totalStartedCoroutines);
				}
				totalStartedCoroutines = totalStartedCoroutines - _totalStartedCoroutines;
				totalTerminatedCoroutines = totalTerminatedCoroutines - _totalTerminatedCoroutines;
				PerfMon.SetMetric(PerfMonConst.NodeCs_Threading_StartedCoroutines, totalStartedCoroutines);
				PerfMon.SetMetric(PerfMonConst.NodeCs_Threading_TerminatedCoroutines, totalTerminatedCoroutines);
				PerfMon.SetMetric(PerfMonConst.NodeCs_Threading_RunningCoroutines, totalRunningCoroutines);
				_totalStartedCoroutines += totalStartedCoroutines;
				_totalTerminatedCoroutines += totalTerminatedCoroutines;
			}
		}

		private void InitializeProviders()
		{
			foreach (var provider in GlobalVars.Settings.DbProviderFactories)
			{
				var type = Type.GetType(provider.ProviderFactoryType);
				try
				{
					var dataSet = System.Configuration.ConfigurationSettings.GetConfig("system.data") as System.Data.DataSet;
					for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
					{
						var item = dataSet.Tables[0].Rows[i];
						if (String.Compare(item[2].ToString(), provider.InvariantName, StringComparison.OrdinalIgnoreCase) == 0)
						{
							dataSet.Tables[0].Rows.Remove(item);
						}
					}
					dataSet.Tables[0].Rows.Add(provider.InvariantName
					, provider.InvariantName
					, provider.InvariantName
					, provider.ProviderFactoryType);
				}
				catch (System.Data.ConstraintException) { }
			}
		}
	}
}
