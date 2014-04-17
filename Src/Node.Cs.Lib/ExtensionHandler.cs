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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Containers.Asyncs;
using ConcurrencyHelpers.Coroutines;
using ExpressionBuilder;
using GenericHelpers;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Loaders;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Static;
using System.Web;
using Node.Cs.Lib.Utils;
using Node.Cs.Lib.Controllers;

namespace Node.Cs.Lib
{
	internal class ExtensionHandler : IExtensionHandler
	{
		private readonly CoroutineMemoryCache _memoryCache;
		private readonly IGlobalPathProvider _pathProvider;
		private readonly HandlersLoader _handlersLoader;
		private readonly List<HandlerDefinition> _definitions;

		private readonly IGlobalExceptionManager _globalExceptionManager;
		private readonly Func<HttpContextBase, PageDescriptor, ICoroutine> _defaultHandler;

		public ReadOnlyCollection<string> Extensions
		{
			get { return _extensions; }
		}

		private readonly AsyncLockFreeDictionary<string, Func<HttpContextBase, PageDescriptor, ICoroutine>> _couroutineHandlers;
		private readonly Dictionary<string, bool> _sessionCapable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private ReadOnlyCollection<string> _extensions;

		public ExtensionHandler(CoroutineMemoryCache memoryCache,
			HandlersLoader handlersLoader)
		{

			_definitions = GlobalVars.Settings.Handlers;
			_globalExceptionManager = GlobalVars.ExceptionManager;
			_memoryCache = memoryCache;
			_pathProvider = GlobalVars.PathProvider;
			_handlersLoader = handlersLoader;

			_defaultHandler = (a, b) =>
							  {
								  var result = new StaticHandler();
								  result.Initialize(a, b, _memoryCache, _globalExceptionManager, GlobalVars.PathProvider);
								  return result;
							  };
			// ReSharper disable once IteratorMethodResultIsIgnored
			_memoryCache.CreateArea(StaticHandler.StaticHandlerCache);
			_memoryCache.AddCoroutine(new CleanUpOnMaxMemoryReached(_memoryCache, StaticHandler.StaticHandlerCache));

			_couroutineHandlers = new AsyncLockFreeDictionary<string, Func<HttpContextBase, PageDescriptor, ICoroutine>>(new Dictionary<string, Func<HttpContextBase, PageDescriptor, ICoroutine>>());

		}

		private void RegisterExtensionHandler(string extension, Func<HttpContextBase, PageDescriptor, ICoroutine> coroutineFactory, bool isSessionCapable)
		{
			_couroutineHandlers.Add(extension, coroutineFactory);
			_sessionCapable.Add(extension, isSessionCapable);
		}

		public bool IsSessionCapable(string extension)
		{
			return _sessionCapable[extension];
		}

		public Func<HttpContextBase, PageDescriptor, ICoroutine> GetHandler(string extension)
		{
			var handler = _defaultHandler;
			if (_couroutineHandlers.ContainsKey(extension))
			{
				handler = _couroutineHandlers[extension];
			}
			return handler;
		}

		public void InitializeHandlers()
		{
			foreach (var handler in _definitions)
			{
				var type = AssembliesManager.LoadType(handler.Handler);
				for (var i = 0; i < handler.Extensions.Count; i++)
				{
					var ext = handler.Extensions[i];
					var function = Function.Create()
						.WithBody(
							CodeLine.CreateVariable<ICoroutine>("result"),
							CodeLine.Assign("result",
								Operation.CreateInstance(type))
						)
						.Returns("result");
					var lambda = function.ToLambda<Func<ICoroutine>>();
					var utilityHandler = (IResourceHandler)lambda();
					RegisterExtensionHandler("." + ext, (a, b) =>
														{
															var result = (IResourceHandler)lambda();
															result.Initialize(a, b, _memoryCache, _globalExceptionManager, _pathProvider);
															return (ICoroutine)result;
														}, utilityHandler.IsSessionCapable);
				}
			}
			Thread.Sleep(2 * _couroutineHandlers.Timer.Period);
			_extensions = new ReadOnlyCollection<string>(_couroutineHandlers.Keys.ToArray());
		}

		public ICoroutine CreateInstance(HttpContextBase context, PageDescriptor foundedPath)
		{
			var ext = Path.GetExtension(foundedPath.RealPath);
			if (ext == null)
			{
				return Coroutine.NullCoroutine;
			}
			var extension = ext.ToLowerInvariant();
			var handlerFactory = GetHandler(extension);
			return handlerFactory(context, foundedPath);
		}
	}
}
