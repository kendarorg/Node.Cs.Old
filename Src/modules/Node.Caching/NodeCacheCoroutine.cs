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


using System.Linq;
using System.Threading;
using ConcurrencyHelpers.Containers;
using CoroutinesLib.Shared;
using NodeCs.Shared.Caching;
using System.Collections.Generic;

namespace Node.Caching
{


	public class NodeCacheCoroutine : CoroutineBase
	{
		
		private readonly ICacheEngine _nodeCache;

		public NodeCacheCoroutine(ICacheEngine nodeCache)
		{
			_nodeCache = nodeCache;
			InstanceName = "NodeCacheCoroutine";

		}

		public override void Initialize()
		{

		}

		public override IEnumerable<ICoroutineResult> OnCycle()
		{
			if (_actionRequested.Data != null)
			{
				HandleActionRequested(_actionRequested.Data);
			}
			foreach (var item in _nodeCache.Execute())
			{
				yield return CoroutineResult.Wait;
			}
		}


		public override void OnEndOfCycle()
		{

		}

		public void Stop()
		{
			TerminateElaboration();
		}

		private readonly LockFreeItem<string> _actionRequested = new LockFreeItem<string>(null);
		private readonly LockFreeItem<object> _actionRequestedResult = new LockFreeItem<object>(null);



		private void HandleActionRequested(string data)
		{
			if (data == null) return;
			if (data == "ListGroups")
			{
				_actionRequestedResult.Data = new Dictionary<string,CacheGroupDefinition>(((NodeCache)_nodeCache).Groups);
				
			}else if (data.StartsWith("ListItems:"))
			{
				var groupId = data.Split('~').Last();
				_actionRequestedResult.Data = new Dictionary<string, CacheDefinition>(((NodeCache)_nodeCache).GetItems(groupId));
			}
			_actionRequested.Data = null;
		}

		public IEnumerable<CacheGroupDefinition> ListGroups()
		{
			_actionRequested.Data = "ListGroups";

			while (_actionRequested.Data == "ListGroups")
			{
				Thread.Sleep(250);
			}
			return _actionRequestedResult.Data as IEnumerable<CacheGroupDefinition>;
		}


		public IEnumerable<CacheDefinition> ListGroupItems(string groupId)
		{
			_actionRequested.Data = "ListItems~" + groupId;

			while (_actionRequested.Data == "ListItems~" + groupId)
			{
				Thread.Sleep(250);
			}
			return _actionRequestedResult.Data as IEnumerable<CacheDefinition>;
		}
	}
}