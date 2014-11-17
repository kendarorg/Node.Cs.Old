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
using System;

namespace RuntimeCaching
{
	public class CachingModule : NodeModuleBase
	{
		private readonly RuntimeCache _nodeCache;

		public CachingModule()
		{
			Log = ServiceLocator.Locator.Resolve<ILogger>();
			var timeout = TimeSpan.FromHours(1);
			_nodeCache = new RuntimeCache();
			SetParameter(HttpParameters.CacheInstance, _nodeCache);
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.RegisterCommand("runtimecache.groups", new CommandDefinition((Action)ShowGroups));
			main.RegisterCommand("runtimecache.items", new CommandDefinition((Action<string>)ShowItems, typeof(string)));
		}

		public override void Initialize()
		{
			var timeout = GetParameter<TimeSpan>(HttpParameters.CacheTimeout);
			_nodeCache.Log = ServiceLocator.Locator.Resolve<ILogger>();
			if (timeout == null)
			{
				throw new NotImplementedException();
			}
			
		}

		protected override void Dispose(bool disposing)
		{
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.UnregisterCommand("runtimecache.groups", 0);
			main.UnregisterCommand("runtimecache.items", 1);
		}

		private void ShowGroups()
		{
			/*NodeRoot.CWriteLine("Current cache groups:");

			NodeRoot.CWriteLine(NodeRoot.Pad("Name", 39) + " " + NodeRoot.Pad("Expire(ms)", 39));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 39, "=") + " " + NodeRoot.Pad("", 39, "="));
			foreach (var item in _nodeCache.Groups.Values)
			{
				NodeRoot.CWriteLine(NodeRoot.Pad(item.Id, 39) + " " + NodeRoot.Pad(item.ExpireAfter.TotalMilliseconds, 39));
			}
			NodeRoot.CWriteLine();*/
			throw new NotImplementedException();
		}

		private void ShowItems(string groupId)
		{
			/*groupId = groupId ?? string.Empty;
			NodeRoot.CWriteLine("Current item for cache group '" + groupId + "':");

			NodeRoot.CWriteLine(NodeRoot.Pad("Name", 80) + " " + NodeRoot.Pad("Expire", 30) + " " + NodeRoot.Pad("Expire", 40));
			NodeRoot.CWriteLine(NodeRoot.Pad("", 80, "=") + " " + NodeRoot.Pad("", 30, "=") + " " + NodeRoot.Pad("", 40, "="));
			foreach (var item in _nodeCache.GetItems(groupId))
			{
				NodeRoot.CWriteLine(NodeRoot.Pad(item.Key, 80) + " " + NodeRoot.Pad(item.Value.NextExpiration, 30) + " " + NodeRoot.Pad(item.Value.Value, 40));
			}
			NodeRoot.CWriteLine();*/
			throw new NotImplementedException();
		}
	}
}
