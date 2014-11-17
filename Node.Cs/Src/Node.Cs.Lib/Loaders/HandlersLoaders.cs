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
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Loaders
{
	public class HandlersLoader
	{
		private readonly Type _coroutineType;
		private readonly Type _handlerType;
		private readonly Type _initializerType;
		private readonly IGlobalPathProvider _pathProvider;
		private readonly CoroutineMemoryCache _memoryCache;
		private readonly Dictionary<string, Type> _handlers;

		public HandlersLoader(string entryAssemblyLocation,CoroutineMemoryCache memoryCache )
		{
			_handlers = new Dictionary<string, Type>();
			_pathProvider = GlobalVars.PathProvider;
			_memoryCache = memoryCache;
			_handlerType = typeof(IResourceHandler);
			_coroutineType = typeof(ICoroutine);
			_initializerType = typeof(IResourceHandlerInitializer);
			var tempBinPaths = new List<string>(GlobalVars.Settings.Paths.BinPaths);
			tempBinPaths.Add(Path.GetDirectoryName(entryAssemblyLocation));
			var defaultAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			foreach (var item in GlobalVars.Settings.Handlers)
			{
				var currentItemDll = string.IsNullOrWhiteSpace(item.Dll)?defaultAssemblyName:item.Dll;
				var loadedAssembly = NodeCsAssembliesManager.LoadIfNotPresent(currentItemDll, tempBinPaths);
				if (loadedAssembly != null)
				{
					LoadHandlersFromAssembly(loadedAssembly);
				}

			}
		}

		private void LoadHandlersFromAssembly(Assembly asm)
		{
			var initializer = asm.GetTypes().FirstOrDefault(t => _initializerType.IsAssignableFrom(t));
			if (initializer == null) return;
			var initializerInstance = (IResourceHandlerInitializer)Activator.CreateInstance(initializer);
			initializerInstance.Initialize(_pathProvider, _memoryCache);
			var types = asm.GetTypes().Where(t => _handlerType.IsAssignableFrom(t) && _coroutineType.IsAssignableFrom(t)).ToList();
			foreach (var type in types)
			{
				_handlers.Add(type.FullName, type);
			}
		}

		public IEnumerable<Type> Handlers
		{
			get
			{
				return _handlers.Values;
			}
		}
	}
}
