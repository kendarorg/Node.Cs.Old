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


using System.Collections.ObjectModel;

namespace ClassWrapper
{
	public class ClassWrapper
	{
		public object Instance { get; private set; }
		private readonly ClassWrapperDescriptor _descriptor;

		internal ClassWrapper(ClassWrapperDescriptor descriptor, object instance)
		{
			Instance = instance;
			_descriptor = descriptor;
		}

		public ClassWrapper(ClassWrapperDescriptor descriptor)
		{
			_descriptor = descriptor;
		}

		public ReadOnlyCollection<string> Properties
		{
			get
			{
				return _descriptor.Properties;
			}
		}

		public ReadOnlyCollection<string> Methods
		{
			get
			{
				return _descriptor.Methods;
			}
		}

		public bool ContainsMethod(string methodName)
		{
			return _descriptor.ContainsMethod(methodName);
		}

		public ReadOnlyCollection<MethodWrapperDescriptor> GetMethodGroup(string methodName)
		{
			return _descriptor.GetMethodGroup(methodName);
		}

		public bool ContainsProperty(string propertyName)
		{
			return _descriptor.ContainsProperty(propertyName);
		}

		public T InvokeReturn<T>(string methodName, params object[] pars)
		{
			return _descriptor.InvokeReturn<T>(Instance, methodName, pars);
		}

		public object InvokeObject(string methodName, params object[] pars)
		{
			return _descriptor.InvokeReturnObject(Instance, methodName, pars);
		}

		public void Invoke(string methodName, params object[] pars)
		{
			_descriptor.Invoke(Instance, methodName, pars);
		}

		public T Get<T>(string methodName)
		{
			return _descriptor.Get<T>(Instance, methodName);
		}

		public bool TryInvoke(MethodWrapperDescriptor meth, out object result, params object[] valuesParams)
		{
			return _descriptor.TryInvoke(Instance, meth, out result, valuesParams);
		}

		public object GetObject(string methodName)
		{
			return _descriptor.GetObject(Instance, methodName);
		}

		public void Set(string methodName, object value)
		{
			_descriptor.Set(Instance, methodName, value);
		}
	}
}
