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

namespace Http.PathProvider.StaticContent
{
	public class StaticContentModule : NodeModuleBase
	{
		private StaticContentPathProvider _pathProvider;
		private INodeModule _cachingModule;

		public override void Initialize()
		{
			var httpModule = ServiceLocator.Locator.Resolve<HttpModule>(); ;
			_pathProvider = new StaticContentPathProvider(GetParameter<string>(HttpParameters.PathProviderConnectionString));
			_pathProvider.SetVirtualDir(httpModule.GetParameter<string>(HttpParameters.HttpVirtualDir));
			SetParameter(HttpParameters.PathProviderInstance, _pathProvider);
			_cachingModule = GetParameter<INodeModule>(HttpParameters.CacheInstance);
			if (_cachingModule != null)
			{
				_pathProvider.SetCachingEngine(_cachingModule.GetParameter<ICacheEngine>(HttpParameters.CacheInstance));
				_pathProvider.InitializeFileWatcher();
			}

			httpModule.RegisterPathProvider(_pathProvider);
			_pathProvider.Log = ServiceLocator.Locator.Resolve<ILogger>();
		}

		protected override void Dispose(bool disposing)
		{
			var httpModule = ServiceLocator.Locator.Resolve<HttpModule>(); ;
			httpModule.UnregisterPathProvider(_pathProvider);
		}
	}
}
