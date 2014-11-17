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
using CoroutinesLib.Shared.Logging;

namespace NodeCs.Shared
{
	public abstract class NodeModuleBase : INodeModule,ILoggable
	{
		protected NodeModuleBase()
		{
			Log = ServiceLocator.Locator.Resolve<ILogger>();
		}
		private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		public abstract void Initialize();
		public void PreInitialize()
		{
			
		}

		public void PostInitialize()
		{
			
		}

		public virtual void SetParameter(string name, object value)
		{
			_parameters[name] = value;
		}

		public virtual void RemoveParameter(string name)
		{
			if (_parameters.ContainsKey(name))
			{
				_parameters.Remove(name);
			}
		}

		public T GetParameter<T>(string name)
		{
			if (!_parameters.ContainsKey(name))
			{
				return default(T);
			}
			return (T)_parameters[name];
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~NodeModuleBase()
		{
			Dispose(false);
		}

		protected abstract void Dispose(bool disposing);

		public ILogger Log { get; set; }
	}
}
