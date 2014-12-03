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
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.PathProviders;
using Http.Shared.Renderers;
using NodeCs.Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace Http.Coroutines
{
	public class RenderizableItemCoroutine : CoroutineBase
	{
		private readonly IRenderer _renderer;
		private readonly string _relativePath;
		private readonly IPathProvider _pathProvider;
		private readonly IHttpContext _context;
		private readonly Func<Exception, IHttpContext, bool> _specialHandler;
		private readonly object _model;
		private readonly ModelStateDictionary _modelStateDictionary;
		private readonly object _viewBag;

		public RenderizableItemCoroutine(IRenderer renderer, string relativePath, IPathProvider pathProvider, 
			IHttpContext context, Func<Exception, IHttpContext, bool> specialHandler, object model, 
			ModelStateDictionary modelStateDictionary, object viewBag)
		{
			InstanceName = "RenderItem(" + renderer.GetType().Namespace + "." + renderer.GetType().Name + "," + relativePath + ")";
			_renderer = renderer;
			_relativePath = relativePath;
			_pathProvider = pathProvider;
			_context = context;
			_specialHandler = specialHandler;
			_model = model;
			_modelStateDictionary = modelStateDictionary;
			_viewBag = viewBag;
		}

		public override void Initialize()
		{

		}
		private bool _iInitializedSession = false;

		public override IEnumerable<ICoroutineResult> OnCycle()
		{
			var completed = false;
			var result = new byte[0];
			var lastModification = DateTime.MaxValue;
			yield return CoroutineResult.RunAndGetResult(_pathProvider.GetStream(_relativePath, _context),
				string.Format("RenderItem::IPathProvider::GetStream('{0}',context)",
						_relativePath))
				.OnComplete<StreamResult>((r) =>
				{
					completed = true;
					result = r.Result;
					lastModification = r.LastModification;
				})
				.WithTimeout(TimeSpan.FromSeconds(60))
				.AndWait();

			if (!completed)
			{
				throw new HttpException(500, string.Format("Error loading '{0}'.", _relativePath));
			}

			Exception thrownException = null;
			var target = new MemoryStream(result);

			if (_renderer.IsSessionCapable)
			{
				if (_context.Session == null)
				{
					var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
					if (sessionManager != null)
					{
						_iInitializedSession = true;
						sessionManager.InitializeSession(_context);
					}
				}
			}

			yield return CoroutineResult.Run(_renderer.Render(_relativePath, lastModification, target,
					_context, _model, _modelStateDictionary, _viewBag),
					string.Format("RenderItem::Render '{0}'", _relativePath))
					.WithTimeout(TimeSpan.FromMinutes(1))
					.OnError((e) =>
					{
						thrownException = e;
						return true;
					})
					.AndWait();
			if (thrownException != null)
			{
				throw new HttpException(500,
					thrownException.Message + ": " + thrownException.GetType().Namespace + thrownException.GetType().Name, thrownException);
			}
			if (_iInitializedSession)
			{
				if (_context.Session != null)
				{
					var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
					sessionManager.SaveSession(_context);
				}
			}
			TerminateElaboration();

		}

		public override void OnEndOfCycle()
		{

		}

		public override bool OnError(Exception exception)
		{
			if (_specialHandler != null) _specialHandler(exception, _context);
			if (_iInitializedSession)
			{
				if (_context.Session != null)
				{
					var sessionManager = ServiceLocator.Locator.Resolve<ISessionManager>();
					sessionManager.SaveSession(_context);
				}
			}

			return true;
		}
	}
}
