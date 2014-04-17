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
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.OnReceive;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Utils;
using Node.Cs.Razor.Helpers;
using RazorEngine.Templating;
using RazorEngine.Compilation;

namespace Node.Cs.Razor
{
	public class RazorHandler : Coroutine, IResourceHandler
	{
		public ModelStateDictionary ModelState { get; set; }
		public Dictionary<string, object> ViewData { get; set; }
		internal const string CACHE_AREA = "Node.Cs.Razor.RazorHandler";
		private NodeCsContext _context;
		private PageDescriptor _pageDescriptor;
		private CoroutineMemoryCache _memoryCache;
		private IGlobalExceptionManager _globalExceptionManager;
		private IGlobalPathProvider _globalPathProvider;

		public bool IsSessionCapable { get { return false; } }
		public dynamic ViewBag { get; set; }
		public object Model { get; set; }

		protected Uri _uri;
		internal string ResultContent;

		internal bool IsChildCall = false;
		private CultureInfo _currentCulture;

		// ReSharper disable once UnusedParameter.Local
		public void Initialize(
			HttpContextBase context,
			PageDescriptor filePath,
			CoroutineMemoryCache memoryCache,
			IGlobalExceptionManager globalExceptionManager,
			IGlobalPathProvider globalPathProvider)
		{
			_currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_context = (NodeCsContext)context;
			_pageDescriptor = filePath;
			_memoryCache = memoryCache;
			_globalExceptionManager = globalExceptionManager;
			_globalPathProvider = globalPathProvider;
			_uri = _context.Request.Url;
		}

		public override IEnumerable<Step> Run()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = _currentCulture;
			System.Text.Encoding encoding;
			try
			{
				encoding = _context.Request.ContentEncoding;
			}
			catch
			{
				encoding = System.Text.Encoding.UTF8;
			}
			encoding = new EncodingWrapper(encoding);
			var localPath = _uri.LocalPath;
			var result = new Container();
			if (_pageDescriptor.PathProvider.IsFileChanged(localPath))
			{
				yield return InvokeLocalAndWait(() =>
					_memoryCache.AddOrReplaceAndGet(localPath, () => ReadCacheData(localPath, Model), CACHE_AREA), result);
			}

			yield return InvokeLocalAndWait(() => _memoryCache.AddOrGet(localPath, () => ReadCacheData(localPath, Model), CACHE_AREA), result);

			dynamic resulting = null;
			if (Model != null)
			{
				resulting = RazorEngine.Razor.Resolve(localPath, Model);
			}
			else
			{
				resulting = RazorEngine.Razor.Resolve(localPath);
			}

			var dwb = new DynamicViewBag();
			if (ViewBag != null)
			{
				var dct = ViewBag as IDictionary<string, object>;
				dwb.AddDictionaryValuesEx(dct);
			}
			dwb.AddValue("NodeCsContext", _context);
			dwb.AddValue("NodeCsLocalPath", localPath);
			dwb.AddValue("ModelState", ModelState);
			dwb.AddValue("ViewData", ViewData);
			var context = new ExecuteContext(dwb);
			var template = resulting as ITemplate;

			ResultContent = template.Run(context);

			var allData = _context.Data;
			_context.Data = new Dictionary<string, object>();
			foreach (var item in allData)
			{
				if (item.Value.GetType().Name.Contains("RenderActionData"))
				{
					var rh = RunRenderAction(item);
					yield return InvokeCoroutineAndWait(rh);
					if (rh.RazorResource == null)
					{
						throw new NodeCsException("Page not found '{0}'.",404,_context.Request.Url.ToString());
					}
					ResultContent = ResultContent.Replace(item.Key, rh.RazorResource.ResultContent);
				}
			}

			if (!IsChildCall)
			{
				byte[] buffer = encoding.GetBytes(ResultContent);
				var output = _context.Response.OutputStream;
				//TODO: Chunked  ((NodeCsResponse) _context.Response).SetContentLength(buffer.Length);
				yield return InvokeTaskAndWait(output.WriteAsync(buffer, 0, buffer.Length));
				_context.Response.Close();
			}
			ShouldTerminate = true;
		}

		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			_globalExceptionManager.HandleException(ex, _context);
			_context.Response.Close();
		}

		//public IEnumerable<object> ReadCacheData(string localPath, object model)
		public IEnumerable<Step> ReadCacheData(string localPath, object model)
		{
			string fileContent = "";
			var container = new Container();
			yield return InvokeLocalAndWait(() => _pageDescriptor.PathProvider.ReadText(_pageDescriptor.RealPath), container);
			fileContent = container.RawData as string;

			if (!IsChildCall)
			{
				string[] possiblePaths = GetPossiblePaths(System.IO.Path.GetDirectoryName(_pageDescriptor.RealPath),
					"_ViewStart.cshtml");
				foreach (var possiblePath in possiblePaths)
				{
					var possible = possiblePath;
					if (_globalPathProvider.FileExists(possible))
					{
						yield return InvokeLocalAndWait(() => _globalPathProvider.ReadText(possible), container);
						var data = container.RawData as string;
						fileContent = data + "\n" + fileContent;
					}
				}
			}
			yield return InvokeTaskAndWait(Task.Run(() =>
				{
					var type = model.GetType();
					if (type.FullName.Contains("System.Data.Entity.DynamicProxies"))
					{
						type = model.GetType().BaseType;
					}
					try
					{
						if (model == null)
						{
							RazorEngine.Razor.Compile(fileContent, localPath);
						}
						else
						{
							RazorEngine.Razor.Compile(fileContent, model.GetType(), localPath);
						}
					}
					catch (TemplateCompilationException ex)
					{
						var toPrint = string.Empty;
						ex.Errors.ToList().ForEach(p =>
														{
															toPrint += p.ErrorText + "\n";
														});
						Logger.Error(ex, "TemplateCompilationException compiling cshtml file: {0}\r\n{1}", localPath, toPrint);
					}
					catch (Exception ex)
					{

						Logger.Error(ex, "Error compiling cshtml file: {0}", localPath);
					}
				}));
			yield return Step.DataStep(localPath);
		}


		//From nearest to farest
		private string[] GetPossiblePaths(string rootDir, string fileToCheck)
		{
			var splitted = rootDir.Split('\\');
			if (splitted.Length == 2)
			{
				return new string[]{
					System.IO.Path.Combine(rootDir,fileToCheck),
					System.IO.Path.Combine(splitted[0],"Shared",fileToCheck),
					System.IO.Path.Combine(splitted[0],fileToCheck)
				};
			}
			if (splitted.Length == 1)
			{
				return new string[]{
					System.IO.Path.Combine(splitted[0],"Shared",fileToCheck),
					System.IO.Path.Combine(splitted[0],fileToCheck)
				};
			}
			return new string[] { };

		}

		private OnRazorReceivedCoroutine RunRenderAction(KeyValuePair<string, object> item)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = _currentCulture;
			var rad = (RenderActionData)item.Value;
			var guid = item.Key;
			var link = GlobalVars.RoutingService.ResolveFromParams(rad.Params);
			((NodeCsRequest)_context.Request).ForceUrl(new Uri("http://localhost" + link));


			
			var rh = new OnRazorReceivedCoroutine(_context);
			rh.ViewData = ViewData;
			rh.Initialize(new RazorContextManager(_context), new SessionManager(),null, null);
			return rh;
		}

		internal static void CleanModelType(TypeContext context)
		{
			if (context.ModelType.FullName.Contains("System.Data.Entity.DynamicProxies"))
			{
				context.ModelType = context.ModelType.BaseType;
			}
		}

	}
}
