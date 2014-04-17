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
using System.IO;
using System.Linq;
using System.Reflection;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Loaders
{
	public class PluginsLoader
	{
		private List<IPluginInitializer> _plugins;
		private IRoutingService _routingService;
		private IGlobalPathProvider _globalPathProvider;
		private static readonly Type _pluginInitializerType = typeof(IPluginInitializer);


		public void Initialize(string entryAssemblyLocation)
		{
			_globalPathProvider = GlobalVars.PathProvider;
			_routingService = GlobalVars.RoutingService;
			_plugins = new List<IPluginInitializer>();
			var tempBinPaths = new List<string>(GlobalVars.Settings.Paths.BinPaths);
			tempBinPaths.Add(Path.GetDirectoryName(entryAssemblyLocation));
			var defaultAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			foreach (var item in GlobalVars.Settings.Plugins)
			{
				var currentItemDll = string.IsNullOrWhiteSpace(item.Dll) ? defaultAssemblyName : item.Dll;
				var loadedAssembly = NodeCsAssembliesManager.LoadIfNotPresent(currentItemDll, tempBinPaths);
				LoadPluginsFromAssembly(loadedAssembly);
			}
		}

		private void LoadPluginsFromAssembly(Assembly asm)
		{
			var initializer = asm.GetTypes().FirstOrDefault(t => _pluginInitializerType.IsAssignableFrom(t));
			if (initializer == null) return;
			var initializerInstance = (IPluginInitializer)Activator.CreateInstance(initializer);
			initializerInstance.Initialize(_routingService);
			initializerInstance.Initialize(_globalPathProvider);
			initializerInstance.Initialize(GlobalVars.ResponseHandlers);
			_plugins.Add(initializerInstance);
		}

		public IEnumerable<IPluginInitializer> Handlers
		{
			get
			{
				return _plugins;
			}
		}
	}
}
