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
using System.IO;
using System.Reflection;
using System.Threading;
using CoroutinesLib;
using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Enums;
using CoroutinesLib.Shared.Logging;
using GenericHelpers;
using NodeCs.Parser;
using NodeCs.Shared;

namespace NodeCs
{
	

	public class Main : MarshalByRefObject, IMain
	{
		private CommandLineParser _clp;
		public string AssemblyDirectory { get; private set; }
		private string _configPath;
		private ICoroutinesManager _coroutinesManager;
		private NodeCsParser _parser;
		private static string _binPath;

		public ICoroutinesManager CoroutinesManager
		{
			get { return _coroutinesManager; }
		}

		public void Initialize(string[] args, string helpMessage)
		{
			AssemblyDirectory = AssembliesManager.GetAssemblyDirectory();
			_clp = new CommandLineParser(args, helpMessage ?? string.Empty);
			_configPath = _clp.GetOrDefault("config", Path.Combine(AssemblyDirectory, "config.json"));
			_binPath = _clp.GetOrDefault("tempBin", Path.Combine(AssemblyDirectory, "bin"));
			

			RunnerFactory.Initialize();
			_coroutinesManager = RunnerFactory.Create();
			_parser = new NodeCsParser(_clp, _configPath, _coroutinesManager);
			NodeRoot.Initialize(this);
		}

		public bool Wait(int ms)
		{
			return WaitHelper.Wait(() => _coroutinesManager.Status.HasFlag(RunningStatus.Stopped), new TimeSpan(0, 0, 0, 0, ms));
		}

		public void Abort()
		{
			_coroutinesManager.Abort();
		}


		static Assembly LoadFromBinPath(object sender, ResolveEventArgs args)
		{
			string assemblyPath = Path.Combine(_binPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			return assembly;
		}




		static Assembly LoadReflectionFromBinPath(object sender, ResolveEventArgs args)
		{
			string assemblyPath = Path.Combine(_binPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			Assembly assembly = Assembly.ReflectionOnlyLoad(assemblyPath);
			return assembly;
		}

		public void Start()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.AssemblyResolve += LoadFromBinPath;
			currentDomain.ReflectionOnlyAssemblyResolve += LoadReflectionFromBinPath;
			_coroutinesManager.Start();
		}

		public void Stop()
		{
			_coroutinesManager.Stop();
		}

		public void StopIncoming()
		{
			_coroutinesManager.AllowIncomingMessage = false;
		}

		public void AllowIncoming()
		{
			_coroutinesManager.AllowIncomingMessage = true;
		}

		public bool Execute(string result)
		{
			return _parser.Execute(result);
		}

		public void RenewLease()
		{

		}

		public void AddThread(Thread thread, string threadName)
		{
			Commands.AddThread(thread, threadName);
		}

		public void RemoveThread(string threadName)
		{
			Commands.RemoveThread( threadName);
		}

		public void RegisterCommand(string name, CommandDefinition definition)
		{
			Commands.RegisterCommand(name, definition);
		}

		public void UnregisterCommand(string name, int paramsCount)
		{
			Commands.UnregisterCommand(name, paramsCount);
		}

		public ILogger Log { get; set; }
	}
}
