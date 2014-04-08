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
using System.Text;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.PathProviders;
using System.Web;
using Node.Cs.Lib.Utils;
using System.IO;
using Node.Cs.Lib.Controllers;

namespace Node.Cs.Lib.Static
{
	public class StaticHandler : Coroutine, IResourceHandler
	{
		public ModelStateDictionary ModelState { get; set; }
		public const string StaticHandlerCache = "Node.Cs.Lib.Static.StaticHandler";
		private HttpContextBase _context;
		private PageDescriptor _pageDescriptor;
		private CoroutineMemoryCache _memoryCache;
		private IGlobalExceptionManager _globalExceptionManager;
		private static readonly HashSet<string> _textFiles;

		public StaticHandler()
		{
			ViewData = new Dictionary<string, object>();
		}
		static StaticHandler()
		{
			
			_textFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				".html",".htm"
			};
		}

		private static bool IsText(string extension)
		{
			return extension != null && _textFiles.Contains(extension);
		}

		public void Initialize(HttpContextBase context, PageDescriptor filePath, CoroutineMemoryCache memoryCache,
			IGlobalExceptionManager globalExceptionManager, IGlobalPathProvider globalPathProvider)
		{
			
			_context = context;
			_pageDescriptor = filePath;
			_memoryCache = memoryCache;
			_globalExceptionManager = globalExceptionManager;
		}

		public IEnumerable<Step> ReadCacheData()
		{
			var result = new Container();
			var extension = Path.GetExtension(_pageDescriptor.RealPath);
			if (!IsText(extension))
			{
				yield return InvokeLocalAndWait(() => _pageDescriptor.PathProvider.ReadBinary(_pageDescriptor.RealPath), result);
				yield return Step.DataStep(result.RawData);
			}
			else
			{
				yield return InvokeLocalAndWait(() => _pageDescriptor.PathProvider.ReadText(_pageDescriptor.RealPath), result);
				yield return Step.DataStep(result.RawData);
			}
		}

		public override IEnumerable<Step> Run()
		{
			if (_context.Request.Url == null) yield break;

			var localPath = _context.Request.Url.LocalPath;
			Container result = new Container();
			if (_pageDescriptor.PathProvider.IsFileChanged(_pageDescriptor.RealPath))
			{
				yield return InvokeLocalAndWait(() => _memoryCache.AddOrReplaceAndGet(localPath, () => ReadCacheData(), NodeCsServer.NodeCsCache), result);
			}

			yield return InvokeLocalAndWait(() => _memoryCache.AddOrGet(localPath, () => ReadCacheData(), NodeCsServer.NodeCsCache), result);
			var foundedItem = result.RawData as CacheItem;

			_context.Response.ContentEncoding = new EncodingWrapper(_context.Request.ContentEncoding);
			var output = _context.Response.OutputStream;
			if (foundedItem != null)
			{
				var byteData = foundedItem.Data as byte[];
				var trim = 0;
				if (byteData == null)
				{
					var stringData = foundedItem.Data as string;
					byteData = Encoding.UTF8.GetBytes(stringData);
				}
				yield return InvokeTaskAndWait(output.WriteAsync(byteData, trim, byteData.Length - trim));
			}
			output.Close();
			ShouldTerminate = true;
		}

		public override void OnError(Exception ex)
		{
			ShouldTerminate = true;
			_globalExceptionManager.HandleException(ex, _context);
			_context.Response.Close();
		}

		public void Initialize(IGlobalPathProvider pathProvider)
		{

		}

		public object Model { get; set; }

		public bool IsSessionCapable { get { return false; } }
		public dynamic ViewBag { get; set; }


		public Dictionary<string, object> ViewData { get; set; }
	}
}
