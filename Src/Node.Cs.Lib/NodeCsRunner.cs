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
using System.Diagnostics;
using System.IO;
using System.Threading;
using ConcurrencyHelpers.Interfaces;
using ConcurrencyHelpers.Utils;
using GenericHelpers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Settings;

namespace Node.Cs.Lib
{
	public class NodeCsRunner : MarshalByRefObject
	{
		private const int RECYCLE_POLL = 1000;

		//static NodeCsSettings _settings;
		private static string _currentPath;
		private static AppDomain _runningAppDomain;
		private NodeCsServer _server;
		private static NodeCsRunner _runner;
		private static SystemTimer _recycleTimer;
		private static Process _process;
		private static string _executableCodeBase;
		private static string[] _args;
		private static string _help;

		public static int MaxMemorySize { get; set; }

		internal int StartInstance(string[] args, string executableCodeBase, string help)
		{
			var clp = new CommandLineParser(args, help);
			try
			{
				_currentPath = Path.GetDirectoryName(executableCodeBase);
				if (string.IsNullOrEmpty(_currentPath))
				{
					throw new NodeCsException("Missing Root Path");
				}

				var settingsFile = ReRootPath("node.config");
				if (clp.Has("config"))
				{
					settingsFile = ReRootPath(clp["config"]);
				}
				Console.WriteLine("Using settings from: {0}.", settingsFile);

				NodeCsSettings nodeCsSettings = null;
				if (!File.Exists(settingsFile))
				{
					_currentPath = Path.GetDirectoryName(settingsFile);
					nodeCsSettings = InitializeSettingsFromCommandLine(clp);
				}
				else
				{
					_currentPath = Path.GetDirectoryName(settingsFile);
				}

				NodeCsConfiguration.InitializeConfigurations(settingsFile, nodeCsSettings);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				clp.ShowHelp();
			}
			var basicNodeCsSetting = new NodeCsSettings();
			try
			{
				basicNodeCsSetting = NodeCsConfiguration.GetSection<NodeCsSettings>("NodeCsSettings");
				_server = new NodeCsServer(basicNodeCsSetting, _currentPath);
				_server.Start();
			}
			catch (System.Reflection.ReflectionTypeLoadException le)
			{
				Console.WriteLine(le);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return basicNodeCsSetting.Threading.MaxMemorySize;
		}

		internal void StopInstance()
		{
			try
			{
				_server.Stop();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}


		private static void CreateDomain()
		{
			_runningAppDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

			var typeToInvoke = typeof(NodeCsRunner);
			_runner = (NodeCsRunner)_runningAppDomain.CreateInstanceAndUnwrap(
				typeToInvoke.Assembly.FullName,
				typeToInvoke.FullName);
		}

		static void OnRecycleTimer(object sender, ElapsedTimerEventArgs e)
		{
			if (_process.PrivateMemorySize64 > MaxMemorySize)
			{
				Recycle();
			}
		}


		static string ReRootPath(string path)
		{
			if (Path.IsPathRooted(path)) return path;
			return Path.Combine(_currentPath.TrimEnd(new[] { '/', '\\' }), path.TrimStart(new[] { '/', '\\' }));
		}

		private static NodeCsSettings InitializeSettingsFromCommandLine(CommandLineParser clp)
		{
			var settings = NodeCsSettings.Defaults(_currentPath);
			if (clp.Has("webpath"))
			{
				settings.Paths.WebPaths[0].FileSystemPath = ReRootPath(clp["webpath"]);
			}
			if (clp.Has("binpath"))
			{
				settings.Paths.BinPaths[0] = ReRootPath(clp["binpath"]);
			}
			if (clp.Has("datadir"))
			{
				settings.Paths.DataDir = ReRootPath(clp["datapath"]);
			}
			if (clp.Has("threads"))
			{
				int threads;
				if (int.TryParse(clp["threads"], out threads))
				{
					settings.Threading.ThreadNumber = threads;
				}
			}
			if (clp.Has("port"))
			{
				int port;
				if (int.TryParse(clp["port"], out port))
				{
					settings.Listener.Port = port;
				}
			}
			return settings;
		}

		public static void Recycle()
		{
			var oldAppDomain = _runningAppDomain;
			var oldRunner = _runner;
			CreateDomain();
			oldRunner = null;
			AppDomain.Unload(oldAppDomain);
			_runner.StartInstance(_args, _executableCodeBase, _help);
			Thread.Sleep(100);
		}

		public static void StartServer(string[] args, string executableCodeBase, string help = "")
		{

			_help = help;
			_args = args;
			_executableCodeBase = executableCodeBase;
			_process = Process.GetCurrentProcess();
			_recycleTimer = new SystemTimer(RECYCLE_POLL, RECYCLE_POLL);
			_recycleTimer.Elapsed += OnRecycleTimer;

			CreateDomain();

			MaxMemorySize = _runner.StartInstance(_args, _executableCodeBase, _help);
			_recycleTimer.Start();

		}

		public static void StopServer()
		{
			_runner.StopInstance();
			AppDomain.Unload(_runningAppDomain);
		}

		public bool IsRunning { get { return _server.IsRunning; } }

		public static void CleanCache()
		{
			_runningAppDomain.SetData(CLEAR_CACHE_COMMAND, CLEAR_CACHE_COMMAND);
		}

		public const string CLEAR_CACHE_COMMAND = "CLEAR_CACHE";
	}

}
