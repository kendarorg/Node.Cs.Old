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


using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Logging;
using GenericHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NodeCs.Shared
{
	public static class NodeRoot
	{
		public static IMain Main { get; private set; }
		public static ILogger Log { get; private set; }


		public static string Pad(object data, int pad, string padVal = " ")
		{
			var str = data == null ? "" : data.ToString();
			if (str.Length > pad)
			{
				str = str.Substring(0, pad - 3) + "...";
			}
			else
			{
				while (str.Length < pad)
				{
					str += padVal;
				}
			}
			return str;
		}

		public static string Lpad(object data, int pad, string padVal = " ")
		{
			var str = data == null ? "" : data.ToString();
			if (str.Length > pad)
			{
				str = str.Substring(0, pad - 3) + "...";
			}
			else
			{
				while (str.Length < pad)
				{
					str = padVal + str;
				}
			}
			return str;
		}


		public static void CClean()
		{
			Console.Clear();
		}

		public static void WriteLine(object data = null)
		{
			var what = data == null ? "" : data.ToString();
			Console.WriteLine("NC>" + what);
		}

		public static void Write(object data = null)
		{
			var what = data == null ? "" : data.ToString();
			Console.Write("NC>" + what);
		}

		public static void CWriteLine(object data = null)
		{
			var what = data == null ? "" : data.ToString();
			Console.WriteLine(what);
		}

		public static void CWrite(object data = null)
		{
			var what = data == null ? "" : data.ToString();
			Console.Write(what);
		}

		private static readonly ConcurrentDictionary<string, INodeModule> _modules =
			new ConcurrentDictionary<string, INodeModule>(StringComparer.OrdinalIgnoreCase);

		public static INodeModule GetModule(string moduleId)
		{
			if (!_modules.ContainsKey(moduleId))
			{
				return null;
			}
			return _modules[moduleId];
		}

		public static INodeModule GetNamedModule(string name, string moduleId)
		{
			if (!_modules.ContainsKey(name + "@" + moduleId))
			{
				if (!_modules.ContainsKey(moduleId))
				{
					return null;
				}
				return _modules[moduleId];
			}
			return _modules[name + "@" + moduleId];
		}

		public static void StartModules()
		{
			var initialized = new HashSet<string>();
			var modules = new List<KeyValuePair<string, INodeModule>>(_modules);
			foreach (var module in modules)
			{
				InitializeModule(module, initialized);
			}
		}

		private static void InitializeModule(KeyValuePair<string, INodeModule> module, HashSet<string> initialized)
		{
			var def = FindDefinition(module.Key);
			var deps = new List<ModuleDependency>(def.Dependencies);
			foreach (var dep in deps)
			{
				var subModule = _modules[dep.Name];
				InitializeModule(new KeyValuePair<string, INodeModule>(dep.Name, subModule), initialized);
			}
			if (initialized.Contains(module.Key))
			{
				return;
			}
			initialized.Add(module.Key);
			NodeRoot.CWriteLine(string.Format("Initializing Module: {0}.", module.Key));
			module.Value.Initialize();

			var runnable = module.Value as IRunnableModule;
			if (runnable != null)
			{
				if (!runnable.IsRunning)
				{
					runnable.Start();
				}
			}
		}

		private static readonly Dictionary<string, ModuleDefinition> _moduleDefinitions =
			new Dictionary<string, ModuleDefinition>(StringComparer.OrdinalIgnoreCase);

		private static LogContainer _logContainer;

		private static ModuleDefinition FindDefinition(string moduleId)
		{
			var atIndex = moduleId.IndexOf("@", StringComparison.OrdinalIgnoreCase);
			if (atIndex > 0)
			{
				moduleId = moduleId.Substring(atIndex + 1);
			}
			if (_moduleDefinitions.ContainsKey(moduleId))
			{
				return _moduleDefinitions[moduleId];
			}
			var reflectionOnly = AssembliesManager.LoadReflectionAssemblyFrom(Main.AssemblyDirectory,
				moduleId + ".dll", false);
			if (reflectionOnly != null)
			{
				var moduleDescriptor = ResourceContentLoader.LoadText(moduleId + ".json", reflectionOnly, false);
				if (moduleDescriptor != null)
				{
					var result = JsonConvert.DeserializeObject<ModuleDefinition>(moduleDescriptor);
					_moduleDefinitions[moduleId] = result;
					if (!string.IsNullOrWhiteSpace(result.ReplaceItem))
					{
						_moduleDefinitions[result.ReplaceItem] = result;
					}
					return result;
				}
			}
			throw new NotImplementedException("Not implemented automatic module download!\r\nMissing module '" + moduleId + "'");
		}

		public static INodeModule LoadNamedModule(string moduleName, string moduleId, string moduleVersion = null)
		{
			ModuleDefinition moduleDefinition = FindDefinition(moduleId);

			foreach (var dep in moduleDefinition.Dependencies)
			{
				LoadNamedModule(moduleName, dep.Name, dep.Version);
			}
			var moduleIndex = moduleName == null ? moduleId : moduleName + "@" + moduleId;
			if (moduleDefinition.Singleton)
			{
				moduleIndex = moduleId;
			}
			var anotherOne = GetModule(moduleIndex);
			if (anotherOne != null)
			{
				return anotherOne;
			}

			if (AssembliesManager.LoadAssemblyFrom(Main.AssemblyDirectory,
				moduleId + ".dll", false))
			{
				var moduleType = AssembliesManager.LoadType(moduleDefinition.Class);
				if (moduleType != null)
				{
					var result = (INodeModule)ServiceLocator.Locator.Resolve(moduleType);
					NodeRoot.CWriteLine(string.Format("Loaded Module: {0} Version: {1}.", Pad(moduleId, 20), moduleVersion ?? "X.X.X.X"));
					_modules[moduleIndex] = result;
					return result;
				}
			}
			throw new NotImplementedException("Not implemented automatic module download!");
		}

		public static IEnumerable<ModuleInstance> Modules
		{
			get
			{
				foreach (var module in _modules)
				{
					yield return new ModuleInstance(module.Key, module.Value);
				}
			}
		}

		public static INodeModule LoadModule(string moduleId, string moduleVersion = null)
		{
			return LoadNamedModule(null, moduleId, moduleVersion);
		}

		public static void Initialize(IMain main)
		{
			Main = main;
			_logContainer = new LogContainer();
			ServiceLocator.Locator.Register<ILogger>(_logContainer);
			ServiceLocator.Locator.Register<IMain>(Main);
			ServiceLocator.Locator.Register<ICoroutinesManager>(Main.CoroutinesManager);
			_logContainer.SetLoggingLevel(_loggingLevel);
			Log = _logContainer;
			Main.Log = _logContainer;
			Main.CoroutinesManager.Log = _logContainer;
		}


		public static void RegisterLogger(ILogger logger)
		{
			_logContainer.RegisterLogger(logger);
		}
		public static void UnregisterLogger(ILogger logger)
		{
			_logContainer.UnregisterLogger(logger);
		}

		public static void LoadDll(string dll)
		{
			AssembliesManager.LoadAssemblyFrom(Main.AssemblyDirectory, dll + ".dll");
		}
		private static LoggerLevel _loggingLevel;
		public static void SetLoggingLevel(string level)
		{
			_loggingLevel = (LoggerLevel)Enum.Parse(typeof(LoggerLevel), level, true);
			_logContainer.SetLoggingLevel(_loggingLevel);
		}

		public static void SetLogFile(string logFilePath)
		{
			_logContainer.SetLogFile(logFilePath);
		}
	}
}
