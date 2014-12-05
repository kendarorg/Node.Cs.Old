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
using CoroutinesLib.Shared.Enums;
using GenericHelpers;
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Renderers;
using NodeCs.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Http.Coroutines
{
	/// <summary>
	/// This class is used to seal all the execution pipeline in one "simple" wrapper,
	/// and to build a true "reentrant" execution.
	/// </summary>
	public class ExecuteRequestCoroutine : ICoroutineThread
	{
		private IHttpContext _context;
		private object _model;
		private ModelStateDictionary _modelStateDictionary;
		private string _virtualDir;
		private readonly List<IPathProvider> _pathProviders;
		private readonly List<IRenderer> _renderers;
		private readonly List<string> _defaulList;
		private readonly object _viewBag;
		private ICoroutineThread _coroutine;


		static readonly Hashtable _respStatus = new Hashtable();
		static ExecuteRequestCoroutine()
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
			ErrorPageString = "{SERVER_TYPE} Detailed Error - {HTTP_CODE} - {SHORT_DESCRIPTION} - {LONG_DESCRIPTION}";
		}

		public ExecuteRequestCoroutine(string virtualDir, IHttpContext context, object model, 
			ModelStateDictionary modelStateDictionary, List<IPathProvider> pathProviders,
			List<IRenderer> renderers, List<string> defaulList, object viewBag)
		{
			_context = context;
			_model = model;
			_modelStateDictionary = modelStateDictionary;
			_virtualDir = virtualDir;
			_pathProviders = pathProviders;
			_renderers = renderers;
			_defaulList = defaulList;
			_viewBag = viewBag;
		}

		public void Initialize()
		{
			if (_context.Request.Url == null)
			{
				throw new HttpException(500, string.Format("Missing url."));
			}
			string requestPath;

			if (_context.Request.Url.IsAbsoluteUri)
			{
				requestPath = _context.Request.Url.LocalPath.Trim();
			}
			else
			{
				requestPath = _context.Request.Url.ToString().Trim();
			}

			var relativePath = requestPath;
			if (requestPath.StartsWith(_virtualDir.TrimEnd('/')))
			{
				if (_virtualDir.Length >0 )
				{
					relativePath = requestPath.Substring(_virtualDir.Length - 1);
				}
			}

			for (int index = 0; index < _pathProviders.Count; index++)
			{
				var pathProvider = _pathProviders[index];
				var isDir = false;
				if (pathProvider.Exists(relativePath, out isDir))
				{
					var renderer = FindRenderer(ref relativePath, pathProvider, isDir);
					if (renderer == null)
					{

						_coroutine = new StaticItemCoroutine(
								relativePath, pathProvider, _context,
								HttpListenerExceptionHandler);
					}
					else
					{
						_coroutine = new RenderizableItemCoroutine(
								renderer,
								relativePath, pathProvider, _context,
								HttpListenerExceptionHandler,
								_model, _modelStateDictionary,
								_viewBag);
					}
					return;
				}
			}
			throw new HttpException(404, string.Format("Not found '{0}'.", _context.Request.Url));
		}

		private IRenderer FindRenderer(ref string relativePath, IPathProvider pathProvider, bool isDir)
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
			var renderer = GetPossibleRenderer(relativePath);
			return renderer;
		}

		private IRenderer GetPossibleRenderer(string relativePath)
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


		public static Exception FindHttpException(Exception exception)
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

		public static void WriteException(IHttpContext context, HttpException httpException)
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

		public static void WriteException(IHttpContext context, int httpCode, Exception exception)
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


		public static string ErrorPageString { get; set; }

		private static void OutputException(ErrorDescriptor errorModel, IHttpContext context)
		{
			var filtersHandler = ServiceLocator.Locator.Resolve<IFilterHandler>();
			var es = ErrorPageString;
			es = es.Replace("{SERVER_TYPE}", errorModel.ServerType);
			es = es.Replace("{HTTP_CODE}", errorModel.HttpCode.ToString());
			es = es.Replace("{SHORT_DESCRIPTION}", errorModel.ShortDescription);
			es = es.Replace("{LONG_DESCRIPTION}", errorModel.LongDescription);
			es = es.Replace("{STACK_TRACE}", errorModel.HttpCode == 500 ? BuildStackTrace(errorModel.Exception) : string.Empty);

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(es);
			context.Response.BinaryWrite(buffer);
		}

		private static string BuildStackTrace(Exception exception)
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



		public Guid Id
		{
			get { return _coroutine.Id; }
		}

		public IEnumerable<ICoroutineResult> Execute()
		{
			_status = RunningStatus.Running;
			foreach (var item in _coroutine.Execute())
			{
				_status = _coroutine.Status;
				yield return item;
			}
			_status = RunningStatus.Stopping;
		}

		private bool _errorHappened = false;
		public bool OnError(Exception exception)
		{
			_errorHappened = true;
			var httpException = exception as HttpException;
			if (httpException != null)
			{
				WriteException(_context, httpException);
			}
			else
			{
				WriteException(_context, 500, exception);
			}
			return true;
		}

		private RunningStatus _status;
		public RunningStatus Status
		{
			get { return _status; }
		}

		public void OnDestroy()
		{
			if (!(_context.Parent is WrappedHttpContext))
			{
				var filtersHandler = ServiceLocator.Locator.Resolve<IFilterHandler>();
				filtersHandler.OnPostExecute(_context);
				_context.Response.Close();
			}/*
			else
			{
				//_context.Response.Close();
			}*/
			_coroutine.OnDestroy();
		}

		public string InstanceName
		{
			get
			{
				return _coroutine.InstanceName;
			}
			set
			{
				_coroutine.InstanceName = value;
			}
		}
	}
}
