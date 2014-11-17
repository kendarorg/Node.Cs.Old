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


using System.Collections.Generic;
using CoroutinesLib.Shared.Logging;
using Http.Shared;
using Http.Shared.Contexts;
using System;
using NodeCs.Shared;

namespace Http
{
	public class FilterHandler : IFilterHandler, ILoggable
	{
		private readonly List<IFilter> _globalFilters = new List<IFilter>();
		public FilterHandler()
		{
			Log = ServiceLocator.Locator.Resolve<ILogger>();
		}
		public void AddFilter(IFilter instance)
		{
			_globalFilters.Add(instance);
		}

		public void AddFilter(Type type)
		{
			throw new NotImplementedException();
		}

		public void RemoveFilter(IFilter instance)
		{
			_globalFilters.Remove(instance);
		}

		public void RemoveFilter(Type type)
		{
			throw new NotImplementedException();
		}

		public bool OnPreExecute(IHttpContext context)
		{
			try
			{
				for (int index = 0; index < _globalFilters.Count; index++)
				{
					var filter = _globalFilters[index];
					if (!filter.OnPreExecute(context))
					{
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return false;
			}

			return true;
		}

		public void OnPostExecute(IHttpContext context)
		{
			try
			{
				for (int index = 0; index < _globalFilters.Count; index++)
				{
					var filter = _globalFilters[index];
					filter.OnPostExecute(context);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		public ILogger Log { get; set; }
	}
}
