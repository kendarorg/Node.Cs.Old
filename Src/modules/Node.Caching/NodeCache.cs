// ======================================NextExpiration=====================
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


using System.Threading;
using System.Threading.Tasks;
using ConcurrencyHelpers.Containers;
using CoroutinesLib;
using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Enums;
using CoroutinesLib.Shared.Logging;
using NodeCs.Shared;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Node.Caching
{
	public class NodeCache : ICacheEngine, ILoggable
	{

		private readonly static TimeSpan _messageTimeout = TimeSpan.FromMinutes(1);
		private readonly ICoroutinesManager _coroutinesManager;

		public Dictionary<string, CacheGroupDefinition> Groups
		{
			get
			{
				var result = new Dictionary<string, CacheGroupDefinition>(StringComparer.InvariantCultureIgnoreCase);
				foreach (var group in _groups)
				{
					result.Add(group.Key, new CacheGroupDefinition
					{
						Capped = group.Value.Capped,
						ExpireAfter = group.Value.ExpireAfter,
						Id = group.Value.Id,
						RollingExpiration = group.Value.RollingExpiration
					});
				}
				return result;
			}
		}

		public NodeCache(ICoroutinesManager coroutinesManager, TimeSpan timeout)
		{
			Log = ServiceLocator.Locator.Resolve<ILogger>();
			_coroutinesManager = coroutinesManager;
			_cacheMessages = new ConcurrentQueue<object>();
			_groups = new ConcurrentDictionary<string, CacheGroup>(StringComparer.OrdinalIgnoreCase);
			_groups.TryAdd(string.Empty, new CacheGroup(new CacheGroupDefinition
			{
				Id = string.Empty,
				Capped = -1,
				ExpireAfter = timeout,
				RollingExpiration = true
			}));
		}

		private readonly ConcurrentQueue<object> _cacheMessages;
		private readonly ConcurrentDictionary<string, CacheGroup> _groups;

		public IEnumerable<ICoroutineResult> Execute()
		{
			object message;
			while (_cacheMessages.TryDequeue(out message))
			{
				HandleMessage((CacheMessage)message);
			}
			foreach (var groupKeyValue in _groups)
			{
				HandleExpiration(groupKeyValue.Value);
			}
			if (_pending.Count == 0)
			{
				yield break;
			}
			var now = DateTime.UtcNow;
			var expired = new List<int>();
			for (int index = 0; index < _pending.Count; index++)
			{
				var pending = _pending[index];
				if (!_groups.ContainsKey(pending.GroupId))
				{
					expired.Add(index);
					pending.HasValue = false;
					pending.IsCompleted = true;
					continue;
				}
				var group = _groups[pending.GroupId];
				if (!group.Items.ContainsKey(pending.ItemId))
				{
					expired.Add(index);
					pending.HasValue = false;
					pending.IsCompleted = true;
					continue;
				}

				var item = group.Items[pending.ItemId];
				if (!item.IsWaiting)
				{
					pending.Value = item.Value;
					pending.HasValue = true;
					pending.IsCompleted = true;
					expired.Add(index);
				}
				else if (pending.Timeout < now)
				{
					expired.Add(index);
					pending.HasValue = false;
					pending.IsCompleted = true;
				}
			}
			if (expired.Count > 0)
			{
				expired.Reverse();
				foreach (var sc in expired)
				{
					_pending.RemoveAt(sc);
				}
			}
		}

		public void AddGroup(CacheGroupDefinition groupDefinition, CacheDefinition cacheItem = null)
		{
			if (_groups.ContainsKey(groupDefinition.Id))
			{
				throw new Exception("Duplicate cache group id " + groupDefinition.Id);
			}
			var cm = new CacheMessage(CacheAction.AddGroup, _messageTimeout)
			{
				GroupDefinition = groupDefinition,
				CacheItem = cacheItem
			};
			_cacheMessages.Enqueue(cm);
		}

		public void InvalidateGroup(string groupId)
		{
			var cm = new CacheMessage(CacheAction.InvalidateGroup, _messageTimeout)
			{
				GroupId = groupId
			};
			_cacheMessages.Enqueue(cm);
		}

		public void RemoveGroup(string groupId)
		{
			var cm = new CacheMessage(CacheAction.RemoveGroup, _messageTimeout)
			{
				GroupId = groupId
			};
			_cacheMessages.Enqueue(cm);
		}

		private void SetupCacheDefinitionExpireTime(CacheDefinition cacheItem, string groupId)
		{
			if (!_groups.ContainsKey(groupId ?? string.Empty))
			{
				throw new Exception("Missing cache group id " + groupId ?? string.Empty);
			}
			var group = _groups[groupId ?? string.Empty];
			if (cacheItem.ExpireAfter.TotalMilliseconds < 0)
			{
				cacheItem.ExpireAfter = group.ExpireAfter;
			}
		}

		public void AddItem(CacheDefinition cacheItem, string groupId = null)
		{
			SetupCacheDefinitionExpireTime(cacheItem, groupId);
			var cm = new CacheMessage(CacheAction.AddItem, _messageTimeout)
			{
				GroupId = groupId,
				CacheItem = cacheItem,
				ItemId = cacheItem.Id
			};
			_cacheMessages.Enqueue(cm);
		}

		public void AddItem(string id, object value, string groupId = null)
		{
			AddItem(new CacheDefinition
			{
				Id = id,
				Value = value,
			}, groupId);
		}

		public void AddOrUpdateItem(CacheDefinition cacheItem, string groupId = null)
		{
			SetupCacheDefinitionExpireTime(cacheItem, groupId);
			var cm = new CacheMessage(CacheAction.AddOrUpdateItem, _messageTimeout)
			{
				GroupId = groupId,
				CacheItem = cacheItem,
				ItemId = cacheItem.Id
			};
			_cacheMessages.Enqueue(cm);
		}

		public void AddOrUpdateItem(string id, object value, string groupId = null)
		{
			AddOrUpdateItem(new CacheDefinition
			{
				Id = id,
				Value = value
			}, groupId);
		}

		public void InvalidateItem(string id, string groupId = null)
		{
			var cm = new CacheMessage(CacheAction.InvalidateItem, _messageTimeout)
			{
				GroupId = groupId,
				ItemId = id
			};
			_cacheMessages.Enqueue(cm);
		}

		private ICoroutineResult BuildResult(CacheMessage cm, Action<object> set)
		{
			return CoroutineResult.RunAndGetResult(WaitForResponse(cm),
				string.Format("NodeCache::BuildResult('{0}',action)", cm.CacheItem.Id))
				.OnComplete((a) => set(a.Result))
				.WithTimeout(_messageTimeout)
				.OnError((a) =>
				{
					Log.Warning("Unable to load " + cm.CacheItem.Id);
					set(null);
					return true;
				}).AndWait();
		}

		public ICoroutineResult AddAndGet(CacheDefinition cacheItem, Action<object> set, string groupId = null)
		{
			SetupCacheDefinitionExpireTime(cacheItem, groupId);
			var cm = new CacheMessage(CacheAction.AddAndGet, _messageTimeout)
			{
				GroupId = groupId,
				CacheItem = cacheItem,
				ItemId = cacheItem.Id
			};
			_cacheMessages.Enqueue(cm);
			return BuildResult(cm, set);
		}

		private IEnumerable<ICoroutineResult> WaitForResponse(CacheMessage cm)
		{
			while (!cm.IsCompleted.Value)
			{
				yield return CoroutineResult.Wait;
			}
			cm.IsCompleted = true;
			if (cm.HasValue)
			{
				yield return CoroutineResult.Return(cm.Value);
			}
			else
			{
				yield return CoroutineResult.Return(null);
			}
		}

		public ICoroutineResult AddOrUpdateAndGet(CacheDefinition cacheItem, Action<object> set, string groupId = null)
		{
			SetupCacheDefinitionExpireTime(cacheItem, groupId);
			var cm = new CacheMessage(CacheAction.AddOrUpdateAndGet, _messageTimeout)
			{
				GroupId = groupId,
				CacheItem = cacheItem,
				ItemId = cacheItem.Id
			};
			_cacheMessages.Enqueue(cm);
			return BuildResult(cm, set);
		}

		public ICoroutineResult Get(string cacheId, Action<object> set, string groupId = null)
		{
			if (!_groups.ContainsKey(groupId ?? string.Empty))
			{
				throw new Exception("Missing cache group id " + groupId ?? string.Empty);
			}
			var cm = new CacheMessage(CacheAction.Get, _messageTimeout)
			{
				GroupId = groupId,
				ItemId = cacheId,
				CacheItem = new CacheItem
				{
					Id = cacheId
				}
			};

			_cacheMessages.Enqueue(cm);
			return BuildResult(cm, set);
		}


		private void HandleExpiration(object value)
		{

		}

		private readonly List<CacheMessage> _pending =
			new List<CacheMessage>();

		private void HandleMessage(CacheMessage message)
		{
			message.GroupId = message.GroupId ?? string.Empty;
			switch (message.Action)
			{
				case (CacheAction.AddGroup):
					HandleAddGroup(message);
					break;
				case (CacheAction.InvalidateGroup):
					HandleInvalidateGroup(message);
					break;
				case (CacheAction.RemoveGroup):
					HandleRemoveGroup(message);
					break;
				case (CacheAction.AddItem):
					HandleAddItem(message);
					break;
				case (CacheAction.AddOrUpdateItem):
					HandleAddOrUpdateItem(message);
					break;
				case (CacheAction.InvalidateItem):
					HandleInvalidateItem(message);
					break;
				case (CacheAction.AddAndGet):
					HandleAddAndGet(message);
					break;
				case (CacheAction.AddOrUpdateAndGet):
					HandleAddOrUpdateAndGet(message);
					break;
				case (CacheAction.Get):
					HandleGet(message);
					break;
				case (CacheAction.UpdateGroupTimeout):
					HandleUpdateGroupTimeout(message);
					break;
				default:
					message.Value = null;
					message.IsCompleted = true;
					break;
			}
		}

		private void HandleAddAndGet(CacheMessage message)
		{
			DoHandleAddUpdateAndGet(message, true, false,true);
		}

		private void DoHandleAddUpdateAndGet(CacheMessage message, bool doGet, bool doUpdate, bool doAdd)
		{
			var expireAfter = message.CacheItem.ExpireAfter;
			if (!_groups.ContainsKey(message.GroupId)) return;
			var group = _groups[message.GroupId];
			if (!group.Items.ContainsKey(message.ItemId) || doUpdate)
			{
				if (!doAdd)
				{
					message.HasValue = false;
					message.IsCompleted = true;
					return;
				}
				var item = new CacheItem
				{
					ExpireAfter = expireAfter,
					IsWaiting = true
				};
				@group.Items[message.ItemId] = item;

				if (message.CacheItem.LoadData == null)
				{
					item.Value = message.CacheItem.Value;
					item.IsWaiting = false;
				}
				else
				{
					if (doGet)
					{
						_pending.Add(message);
					}
					var coroutine = CoroutineResult.RunAndGetResult(message.CacheItem.LoadData(),
						string.Format("NodeCache::HandleAddUpdateGet('{0}',{1},{2})",
						message.CacheItem.Id, doGet, doUpdate))
						.OnComplete<object>((r) =>
						{
							item.Value = r;
							item.IsWaiting = false;
						})
						.WithTimeout(_messageTimeout)
						.OnError((e) =>
						{
							item.IsWaiting = false;
							return true;
						}).AsCoroutine();
					_coroutinesManager.StartCoroutine(coroutine);
				}
			}
			else
			{
				var item = group.Items[message.ItemId];
				if (!item.IsWaiting)
				{

					if (group.RollingExpiration)
					{
						item.ExpireAfter = message.CacheItem.ExpireAfter;
					}
					message.Value = item.Value;
					message.HasValue = true;
					message.IsCompleted = true;
				}
				else
				{
					if (doGet)
					{
						message.CacheItem.ExpireAfter = expireAfter;
						_pending.Add(message);
					}
				}
			}
		}

		private void HandleInvalidateItem(CacheMessage message)
		{
			if (!_groups.ContainsKey(message.GroupId)) return;
			var group = _groups[message.GroupId];
			if (!group.Items.ContainsKey(message.ItemId)) return;
			var item = group.Items[message.ItemId];
			if (!item.IsWaiting)
			{
				group.Items.Remove(message.ItemId);
			}
		}

		private void HandleAddOrUpdateItem(CacheMessage message)
		{
			DoHandleAddUpdateAndGet(message, false, true,true);
		}

		private void HandleAddItem(CacheMessage message, bool update = false)
		{
			DoHandleAddUpdateAndGet(message, false, false,true);
		}

		private void HandleRemoveGroup(CacheMessage message)
		{
			if (_groups.ContainsKey(message.GroupId))
			{
				CacheGroup cg;
				_groups.TryRemove(message.GroupId, out cg);
			}
		}

		private void HandleAddGroup(CacheMessage message)
		{
			var msg = (CacheMessage)message;
			if (msg.GroupDefinition == null) return;
			if (!_groups.ContainsKey(msg.GroupDefinition.Id))
			{
				_groups.TryAdd(msg.GroupDefinition.Id, new CacheGroup(msg.GroupDefinition));
			}
		}


		public void SetGroupTimeout(TimeSpan timeout, string groupId = null)
		{
			if (!_groups.ContainsKey(groupId ?? string.Empty))
			{
				throw new Exception("Missing cache group id " + groupId ?? string.Empty);
			}
			var cm = new CacheMessage(CacheAction.UpdateGroupTimeout, _messageTimeout)
			{
				GroupId = groupId ?? string.Empty,
				GroupTimeout = timeout
			};

			_cacheMessages.Enqueue(cm);
		}

		public Dictionary<string, CacheDefinition> GetItems(string groupId)
		{
			var result = new Dictionary<string, CacheDefinition>(StringComparer.InvariantCultureIgnoreCase);
			groupId = groupId ?? string.Empty;
			if (!_groups.ContainsKey(groupId ?? string.Empty)) return result;

			foreach (var item in _groups[groupId].Items)
			{
				result.Add(item.Key, new CacheDefinition
				{
					ExpireAfter = item.Value.ExpireAfter,
					Id = item.Value.Id,
					Value = item.Value.Value
				});
			}
			return result;
		}

		public ILogger Log { get; set; }

		private void HandleGet(CacheMessage message)
		{
			DoHandleAddUpdateAndGet(message, true, false, false);
		}

		private void HandleAddOrUpdateAndGet(CacheMessage message)
		{
			DoHandleAddUpdateAndGet(message, true, true,true);
		}

		private void HandleInvalidateGroup(CacheMessage message)
		{
			throw new System.NotImplementedException();
		}

		private void HandleUpdateGroupTimeout(CacheMessage message)
		{
			throw new NotImplementedException();
		}
	}
}