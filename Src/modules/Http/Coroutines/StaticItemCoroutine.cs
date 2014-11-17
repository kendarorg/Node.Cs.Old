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
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.PathProviders;
using System;
using System.Collections.Generic;
using System.IO;
using NodeCs.Shared;

namespace Http.Coroutines
{
	public class StaticItemCoroutine : CoroutineBase
	{
		private readonly string _relativePath;
		private readonly IPathProvider _pathProvider;
		private readonly IHttpContext _context;
		private readonly Func<Exception, IHttpContext, bool> _specialHandler;

		public StaticItemCoroutine(string relativePath, IPathProvider pathProvider, IHttpContext context,
			Func<Exception, IHttpContext, bool> specialHandler)
		{
			InstanceName = Guid.NewGuid().ToString().Replace("-","")+" WriteFromPathProvider(" + Path.GetFileName(relativePath) + ")";
			_relativePath = relativePath;
			_pathProvider = pathProvider;
			_context = context;
			_specialHandler = specialHandler;
		}



		public override void Initialize()
		{

		}

		public override IEnumerable<ICoroutineResult> OnCycle()
		{
			var completed = false;
			byte[] result = null;
			var lastModification = DateTime.MaxValue;

			yield return CoroutineResult.RunAndGetResult(_pathProvider.GetStream(_relativePath, _context),
				string.Format("{0}::GetStream('{1}',context)",
						InstanceName,Path.GetFileName(_relativePath)))
				.OnComplete<StreamResult>((r) =>
				{
					completed = true;
					result = r.Result;
					lastModification = r.LastModification;
				})
				.WithTimeout(TimeSpan.FromSeconds(60))
				.AndWait();


			ServiceLocator.Locator.Resolve<IFilterHandler>().OnPostExecute(_context);
			if (!completed || result == null)
			{
				throw new HttpException(500, string.Format("Error loading '{0}'.", _relativePath));
			}

			var target = _context.Response.OutputStream;

			var source = new MemoryStream(result);
			source.Seek(0, SeekOrigin.Begin);
			//Wait that the copy is completed
			try
			{
				source.CopyToAsync(target)
					.ContinueWith((c) => _context.Response.Close());
			}
			catch (Exception)
			{
				
			}

			TerminateElaboration();

		}

		public override bool OnError(Exception exception)
		{
			if (_specialHandler != null) _specialHandler(exception,_context);

			return true;
		}

		public override void OnEndOfCycle()
		{
		}
	}
}
