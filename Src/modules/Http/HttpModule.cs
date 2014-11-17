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


using ClassWrapper;
using ConcurrencyHelpers.Utils;
using CoroutinesLib.Shared;
using GenericHelpers;
using Http.Contexts;
using Http.Controllers;
using Http.Coroutines;
using Http.Shared;
using Http.Shared.Authorization;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Renderers;
using Http.Shared.Routing;
using NodeCs.Shared;
using NodeCs.Shared.Caching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Http
{
	public sealed class HttpModule : NodeModuleBase, IRunnableModule
	{
		private const int WAIT_INCOMING_MS = 100;
		private int _port;
		private string _virtualDir;
		private string _host;
		private Thread _thread;
		private HttpListener _listener;

		private SecurityDefinition _securityDefinition = new SecurityDefinition();

		public HttpModule()
		{
			_errorPageString = ResourceContentLoader.LoadText("errorTemplate.html");
			_conversionService = new ConversionService();
			ServiceLocator.Locator.Register<IConversionService>(_conversionService);
			ServiceLocator.Locator.Register<HttpModule>(this);
			ServiceLocator.Locator.Register<SecurityDefinition>(_securityDefinition);
			_filtersHandler = ServiceLocator.Locator.Resolve<IFilterHandler>();
			if (_filtersHandler == null)
			{
				_filtersHandler = new FilterHandler();
				ServiceLocator.Locator.Register<IFilterHandler>(_filtersHandler);
			}
			SetParameter(HttpParameters.HttpShowDirectoryContent, true);

			RegisterDefaultFiles("index.htm");
			RegisterDefaultFiles("index.html");
		}

		public void SetAuthentication(string authType, string realm, string loginPage)
		{
			_securityDefinition.AuthenticationType = authType;
			_securityDefinition.Realm = realm;
			_securityDefinition.LoginPage = loginPage;
		}

		public override void Initialize()
		{
			_port = GetParameter<int>(HttpParameters.HttpPort);
			_virtualDir = UrlUtils.CleanUpUrl(GetParameter<string>(HttpParameters.HttpVirtualDir));
			if (_virtualDir == string.Empty)
			{
				_virtualDir = "/";
			}
			else
			{
				_virtualDir = "/" + _virtualDir + "/";
			}
			_host = GetParameter<string>(HttpParameters.HttpHost);
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.RegisterCommand("http.show", new CommandDefinition((Action)Httpshow));

		}


		private void Httpshow()
		{
			var listenAddress = string.Format("http://{0}:{1}{2}", _host, _port, _virtualDir);
			Process.Start("IExplore.exe", listenAddress);
		}

		private ConcurrentBool _running;

		public void Start()
		{
			if (IsRunning) return;
			_thread = new Thread(RunHttpModule);
			_running = true;
			_thread.Start();
		}

		public bool IsRunning
		{
			get
			{
				if (_thread == null) return false;
				return _running.Value;
			}
		}

		public void Stop()
		{
			if (_thread == null) return;
			_running = false;
			Thread.Sleep(WAIT_INCOMING_MS * 2);
		}

		private bool _fallBackResult;
		private string _fallBackResultMessage;

		public void RunHttpModule()
		{

			var listenAddress = string.Format("http://{0}:{1}{2}", _host, _port, _virtualDir);
			_listener = new HttpListener();
			_listener.Prefixes.Add(listenAddress);

			_listener.Start();
			NodeRoot.CWriteLine("Started listening from " + listenAddress + ".");

			_fallBackResultMessage = string.Empty;
			_fallBackResult = false;
#if DEBUG_
			if (_routingHandler == null)
			{
				_fallBackResultMessage += "Missing Routing handler.<br>";
				_fallBackResult = true;
			}
			if (_renderers.Count == 0)
			{
				_fallBackResultMessage += "Missing Renderes.<br>";
				_fallBackResult = true;
			}
			if (_pathProviders.Count == 0)
			{
				_fallBackResultMessage += "Missing Path providers.<br>";
				_fallBackResult = true;
			}
#endif
			while (_running.Value)
			{
				var context = _listener.BeginGetContext(ListenerCallback, _listener);
				context.AsyncWaitHandle.WaitOne(WAIT_INCOMING_MS);
			}
			_listener.Close();
			_listener = null;
		}

		protected override void Dispose(bool disposing)
		{
			Stop();
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.UnregisterCommand("https.how", 0);
		}

		public override string ToString()
		{
			return string.Join(":", _host, _virtualDir, _port);
		}

		private void ListenerCallback(IAsyncResult result)
		{
			var listener = (HttpListener)result.AsyncState;
			if (!listener.IsListening) return;
			IHttpContext context = null;
			try
			{
				context = new ListenerHttpContext(listener.EndGetContext(result));
			}
			catch (Exception)
			{
				return;
			}
			if (!IsRunning)
			{
				_filtersHandler.OnPostExecute(context);
				context.Response.Close();
				return;
			}

			try
			{
				if (_fallBackResult)
				{
					var now = DateTime.UtcNow;
					var responseString = "<HTML><BODY>" + _fallBackResultMessage + "Timestamp:" + now + ":" +
							NodeRoot.Lpad(now.Millisecond.ToString(), 4, "0") + "</BODY></HTML>";
					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
					context.Response.BinaryWrite(buffer);
					_filtersHandler.OnPostExecute(context);
					context.Response.Close();
				}
				else
				{
					// ReSharper disable once PossibleNullReferenceException
					ExecuteRequest(context, HttpListenerExceptionHandler);
				}
			}
			catch (HttpException httpException)
			{
				WriteException(context, httpException);
			}
			catch (Exception ex)
			{
				var httpException = FindHttpException(ex);
				if (httpException != null)
				{
					WriteException(context, (HttpException)httpException);
				}
				else
				{
					WriteException(context, 500, ex);
				}

			}
		}

		private Exception FindHttpException(Exception exception)
		{
			if (exception is HttpException)
			{
				return exception;
			}
			if (exception.InnerException != null)
			{
				if (exception.InnerException is HttpException)
				{
					return exception.InnerException;
				}
				var tmpEx = FindHttpException(exception.InnerException);
				if (tmpEx == null) return null;
			}
			return null;
		}

		internal bool HttpListenerExceptionHandler(Exception ex, IHttpContext context)
		{
			var httpException = FindHttpException(ex);
			if (httpException != null)
			{
				WriteException(context, (HttpException)httpException);
			}
			else
			{
				WriteException(context, 500, ex);
			}
			return true;
		}

		internal IControllerWrapperInstance CreateController(RouteInstance instance)
		{
			if (!_controllers.ContainsKey(instance.Controller.Name)) return null;
			var cld = _controllers[instance.Controller.Name];
			var controller = (IController)ServiceLocator.Locator.Resolve(instance.Controller);
			if (controller == null) return null;
			return cld.CreateWrapper(controller);
		}

		public void HandleResponse(IHttpContext context, IResponse response)
		{
			foreach (var handler in _responseHandlers)
			{
				if (handler.CanHandle(response))
				{
					handler.Handle(context, response);
					return;
				}
			}
		}

		public void ExecuteRequest(IHttpContext context, object model, ModelStateDictionary modelStateDictionary)
		{
			var runner = ServiceLocator.Locator.Resolve<ICoroutinesManager>();
			if (context.Request.Url == null)
			{
				throw new HttpException(500, string.Format("Missing url."));
			}
			string requestPath;

			if (context.Request.Url.IsAbsoluteUri)
			{
				requestPath = context.Request.Url.LocalPath.Trim();
			}
			else
			{
				requestPath = context.Request.Url.ToString().Trim();
			}
			var relativePath = requestPath;
			if (requestPath.StartsWith(_virtualDir.TrimEnd('/')))
			{
				relativePath = requestPath.Substring(_virtualDir.Length - 1);
			}

			for (int index = 0; index < _pathProviders.Count; index++)
			{
				var pathProvider = _pathProviders[index];
				var isDir = false;
				if (pathProvider.Exists(relativePath, out isDir))
				{
					if (isDir)
					{
						string tmpPath = null;
						for (int i = 0; i < _defaulList.Count; i++)
						{
							var def = _defaulList[i];
							tmpPath = relativePath.TrimEnd('/') + '/' + def;
							if (pathProvider.Exists(tmpPath, out isDir))
							{
								if (!isDir) break;
							}
							tmpPath = null;
						}
						if (tmpPath != null)
						{
							relativePath = tmpPath;
						}
					}
					var renderer = FindRenderer(relativePath);
					if (renderer == null)
					{
						runner.StartCoroutine(new StaticItemCoroutine(relativePath, pathProvider, context, HttpListenerExceptionHandler));
					}
					else
					{
						runner.StartCoroutine(new RenderizableItemCoroutine(renderer, relativePath, pathProvider, context,
								HttpListenerExceptionHandler, model, modelStateDictionary));
					}
					return;
				}
			}
			throw new HttpException(404, string.Format("Not found '{0}'.", context.Request.Url));
		}

		/// <summary>
		/// Main entry point
		/// </summary>
		/// <param name="context"></param>
		/// <param name="specialHandler"></param>
		internal void ExecuteRequest(IHttpContext context, Func<Exception, IHttpContext, bool> specialHandler)
		{
			var runner = ServiceLocator.Locator.Resolve<ICoroutinesManager>();
			if (context.Request.Url == null)
			{
				throw new HttpException(500, string.Format("Missing url."));
			}
			var requestPath = context.Request.Url.LocalPath.Trim();
			var relativePath = requestPath;
			if (requestPath.StartsWith(_virtualDir.TrimEnd('/')))
			{
				relativePath = requestPath.Substring(_virtualDir.Length - 1);
			}
			_filtersHandler.OnPreExecute(context);
            var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
            if (sessionManager != null)
            {
                sessionManager.InitializeSession(context);
            }
			if (_routingHandler != null)
			{
				var match = _routingHandler.Resolve(relativePath, context);
				if (match != null /*&& match.StaticRoute && !match.BlockRoute*/)
				{
					if (match.BlockRoute)
					{
						WriteException(context, new HttpException(404, "Missing " + context.Request.Url));
						return;
					}
					if (!match.StaticRoute)
					{
						var controller = CreateController(match);
						if (controller != null)
						{
							context.RouteParams = match.Parameters ?? new Dictionary<string, object>();
							controller.Instance.Set("Httpcontext", context);
							runner.StartCoroutine(new RoutedItemCoroutine(match, controller, context, specialHandler, _conversionService,
									null));
							return;
						}
					}
				}

			}
			ExecuteRequest(context, null, new ModelStateDictionary());
		}

		private IRenderer FindRenderer(string relativePath)
		{
			var file = UrlUtils.GetFileName(relativePath);
			var extension = PathUtils.GetExtension(file);
			for (int index = 0; index < _renderers.Count; index++)
			{
				var renderer = _renderers[index];
				if (renderer.CanHandle(extension))
				{
					return renderer;
				}
			}
			return null;
		}

		private readonly List<IPathProvider> _pathProviders = new List<IPathProvider>();
		private readonly List<IRenderer> _renderers = new List<IRenderer>();
		private IRoutingHandler _routingHandler;

		public IRoutingHandler RoutingHandler
		{
			get { return _routingHandler; }
		}

		public void RegisterRouting(IRoutingHandler routingService)
		{
			_routingHandler = routingService;
			ServiceLocator.Locator.Register<IRoutingHandler>(routingService);
			CallClientInitializers();
		}

		public void UnregisterRouting(IRoutingHandler routingService)
		{
			if (_routingHandler == routingService)
			{
				_routingHandler = null;
			}
		}

		public void AddConverter(ISerializer converter, string firstMime, params string[] mimes)
		{
			_conversionService.AddConverter(converter, firstMime, mimes);
		}
		public void RegisterConverter(string mime, ISerializer converter)
		{
			_conversionService.AddConverter(converter, mime);
		}

		public void RegisterPathProvider(IPathProvider pathProvider)
		{
			if (!_pathProviders.Contains(pathProvider))
			{
				var showDirectoryContent = GetParameter<bool>(HttpParameters.HttpShowDirectoryContent);
				pathProvider.ShowDirectoryContent(showDirectoryContent);
				_pathProviders.Add(pathProvider);
			}

		}

		public void UnregisterPathProvider(IPathProvider pathProvider)
		{
			if (_pathProviders.Contains(pathProvider))
			{
				_pathProviders.Remove(pathProvider);
			}
		}

		private readonly List<IResponseHandler> _responseHandlers = new List<IResponseHandler>();

		public void RegisterResponseHandler(IResponseHandler renderer)
		{
			_responseHandlers.Add(renderer);
		}

		public void UnregisterResponseHandler(IResponseHandler renderer)
		{
			if (_responseHandlers.Contains(renderer))
			{
				_responseHandlers.Remove(renderer);
			}
		}
		public void RegisterRenderer(IRenderer renderer)
		{
			if (!_renderers.Contains(renderer))
			{
				_renderers.Add(renderer);
			}
		}

		public void UnregisterRenderer(IRenderer renderer)
		{
			if (_renderers.Contains(renderer))
			{
				_renderers.Remove(renderer);
			}
		}
		private readonly List<string> _defaulList = new List<string>();
		public void RegisterDefaultFiles(string fileName)
		{
			if (!_defaulList.Any((f) => string.Equals(f, fileName, StringComparison.OrdinalIgnoreCase)))
			{
				_defaulList.Add(fileName);
			}
		}

		public void UnregisterDefaultFiles(string fileName)
		{
			var idx = _defaulList.FindIndex((f) => string.Equals(f, fileName, StringComparison.OrdinalIgnoreCase));
			if (idx >= 0)
			{
				_defaulList.RemoveAt(idx);
			}
		}

		private readonly Dictionary<string, ControllerWrapperDescriptor> _controllers =
				new Dictionary<string, ControllerWrapperDescriptor>(StringComparer.OrdinalIgnoreCase);

		private readonly IConversionService _conversionService;
		private string _errorPageString;

		private void CallClientInitializers()
		{
			var par = GetParameter<string>("authenticationType");
			if (!string.IsNullOrEmpty(par)) _securityDefinition.AuthenticationType = par;
			par = GetParameter<string>("realm");
			if (!string.IsNullOrEmpty(par)) _securityDefinition.Realm = par;
			par = GetParameter<string>("loginPage");
			if (!string.IsNullOrEmpty(par)) _securityDefinition.LoginPage = par;

			foreach (var type in AssembliesManager.LoadTypesInheritingFrom<ILocatorInitialize>())
			{
				var initializer = (ILocatorInitialize)ServiceLocator.Locator.Resolve(type);
				initializer.InitializeLocator(ServiceLocator.Locator);
			}
            IAuthenticationDataProvider authenticationDataProvider = null;
			foreach (var type in AssembliesManager.LoadTypesInheritingFrom<IAuthenticationDataProviderFactory>())
			{
				var initializer = (IAuthenticationDataProviderFactory)ServiceLocator.Locator.Resolve(type);
				authenticationDataProvider = initializer.CreateAuthenticationDataProvider();
				ServiceLocator.Locator.Register<IAuthenticationDataProvider>(authenticationDataProvider);
				break;
			}
            if (authenticationDataProvider == null)
            {
                ServiceLocator.Locator.Register<IAuthenticationDataProvider>(NullAuthenticationDataProvider.Instance);
            }
            ISessionManager sessionManager = null;
            foreach (var type in AssembliesManager.LoadTypesInheritingFrom<ISessionManagerFactory>())
            {
                var initializer = (ISessionManagerFactory)ServiceLocator.Locator.Resolve(type);
                sessionManager = initializer.CreateSessionManager();
                ServiceLocator.Locator.Register<ISessionManager>(sessionManager);
                break;
            }
            if (sessionManager == null)
            {
                ServiceLocator.Locator.Register<ISessionManager>(new BasicSessionManager());
            }
            var sessionCache = GetParameter<INodeModule>(HttpParameters.HttpSessionCache);
            if (sessionCache != null)
            {
                ServiceLocator.Locator.Resolve<ISessionManager>().SetCachingEngine(sessionCache.GetParameter<ICacheEngine>(HttpParameters.CacheInstance));

            }
			if (_routingHandler == null) return;
			foreach (var type in AssembliesManager.LoadTypesInheritingFrom<IRouteInitializer>())
			{
				var initializer = (IRouteInitializer)ServiceLocator.Locator.Resolve(type);
				initializer.InitializeRoutes(_routingHandler);
			}
			var controllers = AssembliesManager.LoadTypesInheritingFrom<IController>().ToArray();
			_routingHandler.LoadControllers(controllers);
			foreach (var controller in controllers)
			{
				ServiceLocator.Locator.Register(controller);
				var cld = new ClassWrapperDescriptor(controller, true);
				cld.Load();
				foreach (var method in cld.Methods)
				{
					var methodGroup = cld.GetMethodGroup(method);
					foreach (var meth in methodGroup)
					{
						if (meth.Visibility != ItemVisibility.Public) continue;
						if (meth.Parameters.Count == 0) continue;
						foreach (var param in meth.Parameters)
						{
							var paramType = param.ParameterType;
							if (!paramType.IsValueType && paramType.Namespace != null && !paramType.Namespace.StartsWith("System"))
							{
								ValidationService.RegisterModelType(param.ParameterType);
							}
						}
					}
				}
				_controllers.Add(controller.Name, new ControllerWrapperDescriptor(cld));

			}
			foreach (var type in AssembliesManager.LoadTypesInheritingFrom<IFiltersInitializer>())
			{
				var initializer = (IFiltersInitializer)ServiceLocator.Locator.Resolve(type);
				initializer.InitializeFilters(_filtersHandler);
			}
		}

		static readonly Hashtable _respStatus = new Hashtable();
		private readonly IFilterHandler _filtersHandler;

		static HttpModule()
		{
			_respStatus.Add(200, "Ok");
			_respStatus.Add(201, "Created");
			_respStatus.Add(202, "Accepted");
			_respStatus.Add(204, "No Content");

			_respStatus.Add(301, "Moved Permanently");
			_respStatus.Add(302, "Redirection");
			_respStatus.Add(304, "Not Modified");

			_respStatus.Add(400, "Bad Request");
			_respStatus.Add(401, "Unauthorized");
			_respStatus.Add(403, "Forbidden");
			_respStatus.Add(404, "Not Found");

			_respStatus.Add(500, "Internal Server Error");
			_respStatus.Add(501, "Not Implemented");
			_respStatus.Add(502, "Bad Gateway");
			_respStatus.Add(503, "Service Unavailable");
		}



		private void WriteException(IHttpContext context, HttpException httpException)
		{
			var sd = "Unknown error";
			if (_respStatus.Contains(httpException.Code))
			{
				sd = (string)_respStatus[httpException.Code];
			}
			var errorModel = new ErrorDescriptor()
			{
				Exception = httpException,
				HttpCode = httpException.Code,
				ShortDescription = sd,
				LongDescription = httpException.Message,
				ServerType = "Node.Cs http module V." + Assembly.GetExecutingAssembly().GetName().Version
			};
			OutputException(errorModel, context);
		}

		private void WriteException(IHttpContext context, int httpCode, Exception exception)
		{
			var sd = "Unknown error";
			if (_respStatus.Contains(httpCode))
			{
				sd = (string)_respStatus[httpCode];
			}
			var errorModel = new ErrorDescriptor()
			{
				Exception = exception,
				HttpCode = httpCode,
				ShortDescription = sd,
				LongDescription = exception.Message,
				ServerType = "Node.Cs http module V." + Assembly.GetExecutingAssembly().GetName().Version
			};
			OutputException(errorModel, context);
		}

		private void OutputException(ErrorDescriptor errorModel, IHttpContext context)
		{
			var es = _errorPageString;
			es = es.Replace("{SERVER_TYPE}", errorModel.ServerType);
			es = es.Replace("{HTTP_CODE}", errorModel.HttpCode.ToString());
			es = es.Replace("{SHORT_DESCRIPTION}", errorModel.ShortDescription);
			es = es.Replace("{LONG_DESCRIPTION}", errorModel.LongDescription);
			es = es.Replace("{STACK_TRACE}", errorModel.HttpCode == 500 ? BuildStackTrace(errorModel.Exception) : string.Empty);

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(es);
			context.Response.BinaryWrite(buffer);
			_filtersHandler.OnPostExecute(context);
			context.Response.Close();
		}

		private string BuildStackTrace(Exception exception)
		{
			var result = "<hr size='3'>";
			result += exception.GetType().Namespace + "." + exception.GetType().Name + ":" + exception.Message;
			result += "<hr size='1'>";
			result += "<pre>" + exception.StackTrace + "</pre>";
			var inner = exception.InnerException;
			while (inner != null)
			{
				result += "<hr size='3'>";
				result += inner.GetType().Namespace + "." + inner.GetType().Name + ":" + inner.Message;
				result += "<hr size='1'>";
				result += "<pre>" + inner.StackTrace + "</pre>";
				inner = inner.InnerException;
			}
			return result;
		}
	}
}
