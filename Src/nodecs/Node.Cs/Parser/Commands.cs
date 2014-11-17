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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CoroutinesLib;
using CoroutinesLib.Shared.Logging;
using GenericHelpers;
using NodeCs.Shared;
using SharpTemplate.Compilers;

namespace NodeCs.Parser
{
	internal static class Commands
	{
		internal static Dictionary<string, Thread> _customThreads = new Dictionary<string, Thread>(StringComparer.InvariantCultureIgnoreCase);

		public static readonly ConcurrentDictionary<string, CommandDefinition> Functions = new ConcurrentDictionary<string, CommandDefinition>(
		new Dictionary<string, CommandDefinition>{
			{"run2",new CommandDefinition((Action<string,string>)Commands.Run,typeof(string),typeof(string))},
			{"run1",new CommandDefinition((Action<string>)Commands.Run,typeof(string))},
			{"runthread3",new CommandDefinition((Action<string,string,string>)Commands.RunThread,typeof(string),typeof(string),typeof(string))},
			{"stopthread1",new CommandDefinition((Action<string>)Commands.StopThread,typeof(string))},
			{"listthreads0",new CommandDefinition((Action)Commands.ListThreads)},
			{"exit0",new CommandDefinition(null)},
			{"help1",new CommandDefinition((Action<string>)Commands.Help,typeof(string))},
			{"help0",new CommandDefinition((Action)Commands.Help)},
			{"listdll0",new CommandDefinition((Action)Commands.ListDll)},
			{"listdll1",new CommandDefinition((Action<string>)Commands.ListDll,typeof(string))},
			{"loaddll1",new CommandDefinition((Action<string>)Commands.LoadDll,typeof(string))},
			{"lasterror0",new CommandDefinition(null)},
			{"listmodules0",new CommandDefinition((Action)Commands.ListModules)},
			{"stopmodule2",new CommandDefinition((Action<string,string>)Commands.StopModule,typeof(string),typeof(string))},
			{"stopmodule1",new CommandDefinition((Action<string>)Commands.StopModule,typeof(string))},
			{"startmodule2",new CommandDefinition((Action<string,string>)Commands.StartModule,typeof(string),typeof(string))},
			{"startmodule1",new CommandDefinition((Action<string>)Commands.StartModule,typeof(string))},
			{"loadmodule2",new CommandDefinition((Action<string,string>)Commands.LoadModule,typeof(string),typeof(string))},
			{"loadmodule1",new CommandDefinition((Action<string>)Commands.LoadModule,typeof(string))},
			{"cls0",new CommandDefinition((Action)NodeRoot.CClean)},
			{"listcoroutines0",new CommandDefinition((Action)Commands.RunningCoroutines)},
			{"statuscoroutines0",new CommandDefinition((Action)Commands.RunningStatus)},
			
		});

		private static void LoadModule(string name, string id)
		{
			var module = NodeRoot.LoadNamedModule(name, id);
			module.Initialize();
			NodeRoot.CWriteLine(string.Format("Initialized Module: {0} with name {1}.", id, name));
		}


		private static void LoadModule(string id)
		{
			var module = NodeRoot.LoadModule(id);
			module.Initialize();
			NodeRoot.CWriteLine(string.Format("Initialized Module: {0}.", id));
		}

		private static void StopModule(string id, string name)
		{
			if (name.ToLowerInvariant() == "singleton")
			{
				StopModule(id);
				return;
			}
			var module = NodeRoot.GetNamedModule(name, id) as IRunnableModule;
			if (module == null)
			{
				throw new Exception(string.Format("Module '{0}_{1}' is not runnable or not present.", name, id));
			}
			module.Stop();
		}

		private static void StopModule(string id)
		{
			var module = NodeRoot.GetModule(id) as IRunnableModule;
			if (module == null)
			{
				throw new Exception(string.Format("Module '{0}' is not runnable or not present.", id));
			}
			module.Stop();
		}

		private static void StartModule(string id, string name)
		{
			if (name.ToLowerInvariant() == "singleton")
			{
				StartModule(id);
				return;
			}
			var module = NodeRoot.GetNamedModule(name, id) as IRunnableModule;
			if (module == null)
			{
				throw new Exception(string.Format("Module '{0}_{1}' is not runnable or not present.", name, id));
			}
			if (!module.IsRunning)
			{
				module.Start();
			}
		}

		private static void StartModule(string id)
		{
			var module = NodeRoot.GetModule(id) as IRunnableModule;
			if (module == null)
			{
				throw new Exception(string.Format("Module '{0}' is not runnable or not present.", id));
			}
			if (!module.IsRunning)
			{
				module.Start();
			}
		}

		private static void ListThreads()
		{
			NodeRoot.CWriteLine(NodeRoot.Pad("Name", 30) + " " + NodeRoot.Pad("IsAlive", 10));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 30, "=") + " " + NodeRoot.Pad("", 10, "="));
			foreach (var th in _customThreads)
			{
				NodeRoot.CWriteLine(NodeRoot.Pad(th.Key, 30) + " " + th.Value.IsAlive);
			}
			NodeRoot.CWriteLine();
		}

		private static void ListModules()
		{
			NodeRoot.CWriteLine(NodeRoot.Pad("Name", 30) + " " + NodeRoot.Pad("Run", 5) + " " + NodeRoot.Pad("Instance", 30));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 30, "=") + " " + NodeRoot.Pad("", 5, "=") + " " + NodeRoot.Pad("", 30, "="));
			foreach (var th in NodeRoot.Modules.OrderBy(a => a.Name).ThenBy(a => a.InstanceName))
			{
				var runnable = th.Instance as IRunnableModule;
				if (runnable == null)
				{
					NodeRoot.CWriteLine(NodeRoot.Pad(th.Name, 30) + " " + NodeRoot.Pad("-", 5) + " " + NodeRoot.Pad(th.InstanceName, 30));
				}
				else
				{
					NodeRoot.CWriteLine(NodeRoot.Pad(th.Name, 30) + " " + NodeRoot.Pad(runnable.IsRunning, 5) + " " + NodeRoot.Pad(th.InstanceName, 30));
				}
			}
			NodeRoot.CWriteLine();
		}

		private static void StopThread(string threadName)
		{
			if (!_customThreads.ContainsKey(threadName))
			{
				NodeRoot.WriteLine("Missing thread " + threadName + ".");
				return;
			}
			var th = _customThreads[threadName];
			_customThreads.Remove(threadName);
			if (th.IsAlive)
			{
				th.Abort();
				NodeRoot.WriteLine("Thread " + threadName + " aborted.");
			}
			else
			{
				NodeRoot.WriteLine("Thread " + threadName + " already terminated.");
			}
			NodeRoot.CWriteLine();
		}

		private static void RunThread(string threadName, string script, string function)
		{
			if (_customThreads.ContainsKey(threadName))
			{
				throw new Exception("Duplicate thread name " + threadName + ".");
			}

			var thread = new Thread(DoRunThread);
			thread.Start(new[] { script, function });
			_customThreads.Add(threadName, thread);
		}

		private static void DoRunThread(object par)
		{
			var pars = (string[])par;
			Run(pars[0], pars[1]);
		}

		private static void LoadDll(string path)
		{
			if (Path.GetExtension(path) == string.Empty)
			{
				path = path + ".dll";
			}
			AppDomain.CurrentDomain.Load(File.ReadAllBytes(path));
		}

		private readonly static Dictionary<string, RunnableDefinition> _runnables =
			new Dictionary<string, RunnableDefinition>(StringComparer.InvariantCultureIgnoreCase);


		private static void Run(string path)
		{
			Run(path, "execute");
		}

		public static void Run(string path, string command)
		{
			if (!path.ToLowerInvariant().EndsWith(".cs"))
			{
				path = path + ".cs";
			}

			var loadedAssembly = string.Empty;
			var dllName = Guid.NewGuid().ToString().Replace("-", "");
			var className = dllName + "_class";
			var namespaceName = dllName + "_namespace";
			try
			{
				Type type;
				if (_runnables.ContainsKey(path))
				{
					var def = _runnables[path];
					if (def.Timestamp < File.GetLastWriteTime(path))
					{
						_runnables.Remove(path);
					}
				}

				if (!_runnables.ContainsKey(path))
				{
					var source = File.ReadAllText(path);
					var sc = new SourceCompiler(dllName, "tmp")
					{
						UseAppdomain = true
					};

					sc.AddFile(namespaceName, className, source);
					sc.LoadCurrentAssemblies();
					loadedAssembly = sc.Compile(0);

					if (sc.HasErrors)
					{
						var errs = new HashSet<string>();
						var compilationErrors = "Error compiling " + path;
						foreach (var errorList in sc.Errors)
						{
							var singleErrorList = errorList;
							foreach (var error in singleErrorList)
							{
								if (!errs.Contains(error))
								{
									compilationErrors += "\r\n" + error;
									errs.Add(error);
								}
							}
						}
						throw new Exception(compilationErrors.Replace("\t", "  "));
					}

					var content = File.ReadAllBytes(loadedAssembly);
					var compileSimpleObjectAsm = Assembly.Load(content);
					type =
						compileSimpleObjectAsm.GetTypes()
							.FirstOrDefault(a => a.GetMethods().Any(m => m.Name.ToLowerInvariant() == command.ToLowerInvariant()));
					_runnables.Add(path, new RunnableDefinition
					{
						Type = type,
						Timestamp = File.GetLastWriteTime(path)
					});
				}
				else
				{
					var def = _runnables[path];
					type = def.Type;
				}
				var instance = Activator.CreateInstance(type);
				var methodInfo = instance.GetType().GetMethods().First(m => m.Name.ToLowerInvariant() == command.ToLowerInvariant());
				methodInfo.Invoke(instance, new object[] { });
			}
			finally
			{
				if (File.Exists(loadedAssembly))
				{
					File.Delete(loadedAssembly);
				}
			}
		}

		public static void Help()
		{
			var text = ResourceContentLoader.LoadText("HelpEmpty.txt");
			NodeRoot.CWriteLine(text.Replace("\t", "  "));
			NodeRoot.CWriteLine();
		}

		public static void Help(string command)
		{
			command = command.ToLowerInvariant().Trim();
			if (Functions.ContainsKey(command))
			{
				NodeRoot.CWriteLine(Functions[command].Help);
			}
			else
			{
				var possibleKeys = Functions.Keys.Where(a => a.StartsWith(command)).ToList();
				if (!possibleKeys.Any())
				{
					NodeRoot.CWriteLine(string.Format("Command '{0}' is not present.", command));
					Help();
				}
				else if (possibleKeys.Count() == 1)
				{
					NodeRoot.CWriteLine(Functions[command].Help.Replace("\t", "  "));
				}
				else
				{
					NodeRoot.CWriteLine(string.Format("Available helps for '{0}':", command));
					foreach (var key in possibleKeys)
					{
						NodeRoot.CWriteLine(string.Format("  {0}", key));
					}
				}
			}
			NodeRoot.CWriteLine();
		}

		public static void ListDll()
		{
			NodeRoot.CWriteLine("Assemblies loaded:");

			NodeRoot.CWriteLine(NodeRoot.Pad("Assembly Name", 55) + " " + NodeRoot.Pad("Version", 20));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 55, "=") + " " + NodeRoot.Pad("", 20, "="));

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name))
			{
				if (!asm.IsDynamic && !asm.ReflectionOnly)
				{
					NodeRoot.CWriteLine(NodeRoot.Pad(asm.GetName().Name, 55) + " " + NodeRoot.Pad(asm.GetName().Version.ToString(), 20));
				}
			}
			NodeRoot.CWriteLine();
		}

		public static void ListDll(string par)
		{
			NodeRoot.CWriteLine(string.Format("Assemblies loaded containing '{0}':", par));

			NodeRoot.CWriteLine(NodeRoot.Pad("Assembly Name", 55) + " " + NodeRoot.Pad("Version", 20));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 55, "=") + " " + NodeRoot.Pad("", 20, "="));
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name))
			{
				if (!asm.IsDynamic && !asm.ReflectionOnly && asm.GetName().Name.IndexOf(par, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					NodeRoot.CWriteLine(NodeRoot.Pad(asm.GetName().Name, 55) + " " + NodeRoot.Pad(asm.GetName().Version.ToString(), 20));
				}
			}
			NodeRoot.CWriteLine();
		}

		internal static void AddThread(Thread thread, string threadName)
		{
			if (_customThreads.ContainsKey(threadName))
			{
				throw new Exception("Duplicate thread name " + threadName + ".");
			}

			thread.Start();
			_customThreads.Add(threadName, thread);
		}

		public static void RemoveThread(string threadName)
		{
			StopThread(threadName);
		}

		public static void RegisterCommand(string name, CommandDefinition definition)
		{
			var fullName = name + definition.ParametersCount;
			if (Functions.ContainsKey(fullName))
			{
				return;
			}
			Functions.TryAdd(fullName, definition);
		}

		public static void UnregisterCommand(string name, int paramsCount)
		{
			var fullName = name + paramsCount;
			if (!Functions.ContainsKey(fullName))
			{
				return;
			}
			CommandDefinition cd;
			Functions.TryRemove(fullName, out cd);
		}

		private static void RunningCoroutines()
		{
			NodeRoot.CWriteLine("Running coroutines:");

			NodeRoot.CWriteLine(NodeRoot.Pad("Status", 10) + " " + NodeRoot.Pad("Name", 55));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 10, "=") + " " + NodeRoot.Pad("", 55, "="));
			foreach (var item in NodeRoot.Main.CoroutinesManager.ListCoroutines())
			{
#if DEBUG
				NodeRoot.CWriteLine(NodeRoot.Pad(item.Status, 10) + " " + NodeRoot.Pad(item.InstanceName, 55));
#else
				NodeRoot.CWriteLine(NodeRoot.Pad(item.Status, 10) + " " + NodeRoot.Pad(item.ToString(), 55));
#endif
			}
			NodeRoot.CWriteLine();
		}
		private static void RunningStatus()
		{
			NodeRoot.CWriteLine("Running coroutines status");

			NodeRoot.CWriteLine(NodeRoot.Pad("", 40, "="));
			foreach (var item in NodeRoot.Main.CoroutinesManager.ListRunningStatus())
			{
				NodeRoot.CWriteLine("  " + item);
			}
			NodeRoot.CWriteLine();
		}
	}
}