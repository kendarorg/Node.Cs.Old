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
using System.Diagnostics;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.Utils
{
	public class CleanUpOnMaxMemoryReached : Coroutine
	{
		private readonly int _maxMemorySize;
		private readonly CoroutineMemoryCache _memoryCache;
		private readonly string _cacheArea;
		private readonly Process _process;

		public CleanUpOnMaxMemoryReached(CoroutineMemoryCache memoryCache, string cacheArea)
		{
			_maxMemorySize = GlobalVars.Settings.Threading.MaxMemorySize;
			_memoryCache = memoryCache;
			_cacheArea = cacheArea;
			_process = Process.GetCurrentProcess();
		}

		public override IEnumerable<Step> Run()
		{
			while (Thread.Status != CoroutineThreadStatus.Stopped)
			{
				if (_process.PrivateMemorySize64 > (_maxMemorySize*0.75) || AppDomain.CurrentDomain.GetData(NodeCsRunner.CLEAR_CACHE_COMMAND) != null)
				{
					AppDomain.CurrentDomain.SetData(NodeCsRunner.CLEAR_CACHE_COMMAND, null);
					_memoryCache.Clear(_cacheArea);
				}
				yield return Step.Current;
			}
		}

		public override void OnError(Exception ex)
		{

		}
	}
}