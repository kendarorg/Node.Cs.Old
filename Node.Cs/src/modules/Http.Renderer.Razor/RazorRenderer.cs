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
using System.Diagnostics;
using System.Threading.Tasks;
using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Logging;
using GenericHelpers;
using Http.Renderer.Razor.Integration;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Renderers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NodeCs.Shared.Caching;

namespace Http.Renderer.Razor
{
	public class RazorRenderer : IRenderer, ILoggable
	{
		private const string RAZOR_CACHE_ID = "RazorRenderer";
		public RazorRenderer()
		{
			_renderer = new RazorTemplateGenerator();
		}

		private ICacheEngine _cacheEngine;
		private readonly RazorTemplateGenerator _renderer;

		public bool CanHandle(string extension)
		{
			return extension.ToLowerInvariant() == "cshtml";
		}


		public IEnumerable<ICoroutineResult> Render(string itemPath, DateTime lastModification,
			MemoryStream source, IHttpContext context, object model,
			ModelStateDictionary modelStateDictionary)
		{
			if (_cacheEngine != null)
			{
				StreamResult streamResult = null;
				yield return _cacheEngine.Get(
					itemPath, (a) =>
					{
						streamResult = (StreamResult)a;
					}, RAZOR_CACHE_ID);

				if (streamResult == null || streamResult.LastModification < lastModification)
				{
					yield return _cacheEngine.AddOrUpdateAndGet(new CacheDefinition
					{
						Id = itemPath,
						LoadData = () => RunAsTask(itemPath, source, lastModification, model),
						ExpireAfter = TimeSpan.FromSeconds(60)
					}, (a) =>
					{
						streamResult = (StreamResult)a;
					}, RAZOR_CACHE_ID);
				}

			}
			else
			{
				yield return CoroutineResult.RunTask(Task.Run(() => LoadTransformedBytes(itemPath, source, model)))
					.WithTimeout(TimeSpan.FromSeconds(60))
					.AndWait();
			}
			context.Response.ContentType = MimeHelper.HTML_MIME;

			var stringResult = _renderer.GenerateOutputString(model, itemPath, context, modelStateDictionary);
			var bytes = Encoding.UTF8.GetBytes(stringResult);

			var newSoure = new MemoryStream(bytes);
			var target = context.Response.OutputStream;
			yield return CoroutineResult.RunTask(newSoure.CopyToAsync(target),
				string.Format("RazorRenderer::CopyStream '{0}'", context.Request.Url))
				.AndWait();
		}

		public bool IsSessionCapable
		{
			get { return true; }
		}

		private IEnumerable<ICoroutineResult> RunAsTask(string itemPath, MemoryStream source, DateTime lastModification, object model)
		{
			byte[] bytes = null;
			var task = Task.Run(() => LoadTransformedBytes(itemPath, source, model));
			while (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
			{
				yield return CoroutineResult.Wait;
			}
			yield return CoroutineResult.Return(new StreamResult(lastModification, bytes));
		}

		private void LoadTransformedBytes(string itemPath, MemoryStream source, object model)
		{
			try
			{
				var text = PathUtils.RemoveBom(Encoding.UTF8.GetString(source.ToArray()));
				_renderer.RegisterTemplate(text, itemPath);
				_renderer.CompileTemplates();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading {0}", itemPath);
				throw;
			}
		}

		public void SetCachingEngine(ICacheEngine cacheEngine)
		{
			_cacheEngine = cacheEngine;
			_cacheEngine.AddGroup(new CacheGroupDefinition
			{
				ExpireAfter = TimeSpan.FromDays(1),
				Id = RAZOR_CACHE_ID,
				RollingExpiration = true
			});
		}

		public ILogger Log { get; set; }
	}
}
