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


using System.Collections;
using System.Collections.Generic;
using CoroutinesLib.Shared;
using System;

namespace NodeCs.Shared.Caching
{
	public interface ICacheEngine
	{
		/// <summary>
		/// Add group definition.
		/// Eventually add an item to it
		/// </summary>
		/// <param name="groupDefinition">The cache group definition</param>
		/// <param name="cacheItem">An eventual value</param>
		void AddGroup(CacheGroupDefinition groupDefinition, CacheDefinition cacheItem = null);

		/// <summary>
		/// Invalidate group
		/// </summary>
		/// <param name="groupId"></param>
		void InvalidateGroup(string groupId);

		/// <summary>
		/// Invalidate group
		/// </summary>
		/// <param name="groupId"></param>
		void RemoveGroup(string groupId);

		/// <summary>
		/// Override the default timeout for a group
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="groupId"></param>
		void SetGroupTimeout(TimeSpan timeout, string groupId = null);

		/// <summary>
		/// Add an item
		/// </summary>
		/// <param name="cacheItem"></param>
		/// <param name="groupId"></param>
		void AddItem(CacheDefinition cacheItem, string groupId = null);

		/// <summary>
		/// Add an item
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <param name="groupId"></param>
		void AddItem(string id, object value, string groupId = null);

		/// <summary>
		/// Add an item, refresh if rolling
		/// </summary>
		/// <param name="cacheItem"></param>
		/// <param name="groupId"></param>
		void AddOrUpdateItem(CacheDefinition cacheItem,  string groupId = null);

		/// <summary>
		/// Add an item, refresh if rolling
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <param name="groupId"></param>
		void AddOrUpdateItem(string id, object value, string groupId = null);

		/// <summary>
		/// Invalidate a cache entry
		/// </summary>
		/// <param name="id"></param>
		/// <param name="groupId"></param>
		void InvalidateItem(string id, string groupId = null);

		/// <summary>
		/// Add an item, and get it. Refresh if rolling.
		/// Should be used when a function is needed to get the value
		/// </summary>
		/// <param name="cacheItem"></param>
		/// <param name="groupId"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		ICoroutineResult AddAndGet(CacheDefinition cacheItem, Action<object> set, string groupId = null);


		/// <summary>
		/// Add an item, and get it. Refresh if rolling.
		/// Should be used when a function is needed to get the value
		/// </summary>
		/// <param name="cacheItem"></param>
		/// <param name="groupId"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		ICoroutineResult AddOrUpdateAndGet(CacheDefinition cacheItem, Action<object> set, string groupId = null);

		/// <summary>
		/// Retrieve an item
		/// </summary>
		/// <param name="cacheId"></param>
		/// <param name="groupId"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		ICoroutineResult Get(string cacheId, Action<object> set, string groupId = null);

		/// <summary>
		/// Elaborate data
		/// </summary>
		/// <returns></returns>
		IEnumerable<ICoroutineResult> Execute();

	}
}
