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


using CoroutinesLib.Shared.Logging;
using Http.Shared;
using NodeCs.Shared;
using NodeCs.Shared.Caching;

namespace Http.Renderer.Razor
{
	public class RazorRendererModule : NodeModuleBase
	{
		private RazorRenderer _renderer;
		private INodeModule _cachingModule;
		private RazorViewHandler _handler;

		public override void Initialize()
		{
			var httpModule = ServiceLocator.Locator.Resolve<HttpModule>(); ;
			_renderer = new RazorRenderer();
			_renderer.Log = ServiceLocator.Locator.Resolve<ILogger>();
			_cachingModule = GetParameter<INodeModule>(HttpParameters.CacheInstance);
			if (_cachingModule != null)
			{
				_renderer.SetCachingEngine(_cachingModule.GetParameter<ICacheEngine>(HttpParameters.CacheInstance));
			}
			ServiceLocator.Locator.Register<RazorRenderer>(_renderer);
			SetParameter(HttpParameters.RendererInstance, _renderer);
			httpModule.RegisterRenderer(_renderer);
			_handler = new RazorViewHandler();
			httpModule.RegisterResponseHandler(_handler);
			httpModule.RegisterDefaultFiles("default.cshtml");
			httpModule.RegisterDefaultFiles("index.cshtml");
		}

		protected override void Dispose(bool disposing)
		{
			var httpModule = ServiceLocator.Locator.Resolve<HttpModule>(); ;
			httpModule.UnregisterRenderer(_renderer);
			httpModule.UnregisterResponseHandler(_handler);
			httpModule.UnregisterDefaultFiles("default.cshtml");
			httpModule.UnregisterDefaultFiles("index.cshtml");
		}
	}
}
