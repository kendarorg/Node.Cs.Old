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
using System.Net;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.PathProviders;

namespace Node.Cs.Lib.StaticSpecifics
{
	class StaticHandler : Coroutine, IResourceHandler
	{
		private readonly HttpListenerContext _context;
		private readonly PageDescriptor _pageDescriptor;
		private readonly CoroutineMemoryCache _memoryCache;

		public StaticHandler(HttpListenerContext context, PageDescriptor filePath, CoroutineMemoryCache memoryCache)
		{
			_context = context;
			_pageDescriptor = filePath;
			_memoryCache = memoryCache;
		}

		public IEnumerable<object> ReadCacheData()
		{
			foreach (var step in _pageDescriptor.PathProvider.ReadBinary(_pageDescriptor.RealPath))
			{
				if (step != null)
				{
					yield return step.GetData<byte[]>();
					yield break;
				}
				yield return null;
			}
		}

		public override IEnumerable<Step> Run()
		{
			var localPath = _context.Request.Url.LocalPath;
			CacheItem foundedItem = null;
			// ReSharper disable once ConvertClosureToMethodGroup
			foreach (var step in _memoryCache.AddOrGet(localPath, () => ReadCacheData()))
			{
				if (step == null)
				{
					yield return Step.Current;
				}
				else
				{
					foundedItem = step.GetData<CacheItem>();
					break;
				}
			}
			yield return Step.Current;
			var output = _context.Response.OutputStream;
			if (foundedItem != null)
			{
				var buffer = (byte[])foundedItem.Data;

				foreach (var step in ExecuteAsyncTask(output.WriteAsync(buffer, 0, buffer.Length)))
				{
					yield return step;
				}
			}
			output.Close();
			ShouldTerminate = true;
		}

		public override void OnError(Exception ex)
		{

		}

		public void Initialize(IGlobalPathProvider pathProvider)
		{
			
		}
	}
}
