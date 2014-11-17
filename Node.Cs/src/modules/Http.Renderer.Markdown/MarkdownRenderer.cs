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
using ConcurrencyHelpers.Utils;
using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Logging;
using GenericHelpers;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Renderers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NodeCs.Shared.Caching;

namespace Http.Renderer.Markdown
{
	public class MarkdownRenderer : IRenderer, ILoggable
	{
		private const string MARKDOWN_CACHE_ID = "MarkdownRenderer";
		public MarkdownRenderer()
		{
			_renderer = new MarkdownSharp.Markdown();
			_locker = new ConcurrentInt64();
		}

		private ICacheEngine _cacheEngine;
		private readonly MarkdownSharp.Markdown _renderer;
		private readonly ConcurrentInt64 _locker;

		public bool CanHandle(string extension)
		{
			return extension.ToLowerInvariant() == "md";
		}


		public IEnumerable<ICoroutineResult> Render(string itemPath, DateTime lastModification, 
			MemoryStream source, IHttpContext context, object model,
			ModelStateDictionary modelStateDictionary)
		{
			var bytes = new byte[] { };
			if (_cacheEngine != null)
			{
				StreamResult streamResult = null;
				yield return _cacheEngine.Get(
					itemPath, (a) =>
					{
						streamResult = (StreamResult)a;
					}, MARKDOWN_CACHE_ID);

				if (streamResult == null || streamResult.LastModification < lastModification)
				{
					yield return _cacheEngine.AddOrUpdateAndGet(new CacheDefinition
					{
						Id = itemPath,
						LoadData = () => LoadTransformedBytes(itemPath, source, lastModification),
						ExpireAfter = TimeSpan.FromSeconds(60)
					}, (a) =>
					{
						streamResult = (StreamResult)a;
					}, MARKDOWN_CACHE_ID);
				}

				bytes = streamResult.Result;
			}
			else
			{
				yield return CoroutineResult.RunAndGetResult(LoadTransformedBytes(itemPath, source, lastModification))
					.OnComplete((a) =>
					{
						// ReSharper disable once SuspiciousTypeConversion.Global
						//bytes = ((StreamResult)a).Result;
						bytes = ((StreamResult)a.Result).Result;
					})
					.WithTimeout(TimeSpan.FromSeconds(60))
					.AndWait();
			}
			context.Response.ContentType = MimeHelper.HTML_MIME;
			var newSoure = new MemoryStream(bytes);
			var target = context.Response.OutputStream;
			yield return CoroutineResult.RunTask(newSoure.CopyToAsync(target),
				string.Format("MarkdownRenderer::CopyStream '{0}'", context.Request.Url))
				.AndWait();
		}

		private IEnumerable<ICoroutineResult> LoadTransformedBytes(string itemPath, MemoryStream source, DateTime lastModification)
		{
			string result;
			var text = PathUtils.RemoveBom(Encoding.UTF8.GetString(source.ToArray()));
			while (_locker.Value != 0)
			{
				yield return CoroutineResult.Wait;
			}
			_locker.Increment();
			try
			{
				result = _renderer.Transform(text);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error loading {0}", itemPath);
				throw;
			}
			finally
			{
				_locker.Value = 0;
			}

			var bytes = Encoding.UTF8.GetBytes(result);
			yield return CoroutineResult.Return(new StreamResult(lastModification, bytes));
		}

		public void SetCachingEngine(ICacheEngine cacheEngine)
		{
			_cacheEngine = cacheEngine;
			_cacheEngine.AddGroup(new CacheGroupDefinition
			{
				ExpireAfter = TimeSpan.FromDays(1),
				Id = MARKDOWN_CACHE_ID,
				RollingExpiration = true
			});
		}

		public ILogger Log { get; set; }
	}
}
