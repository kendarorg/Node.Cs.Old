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
using System.Reflection;

namespace Commons.Ioc
{
	public abstract class IocInstance : IIoc
	{
		private static IIoc _instance;

		public static IIoc GetInstance()
		{
			return _instance;
		}

		protected static void SetInstance(IIoc value)
		{
			if (_instance != null)
				{
					throw new ContainerException("Ioc Container already initialized");
				}
				_instance = value;
		}

		public void Register(params IFinalRegistration[] registrations)
		{
			var currentAssembly = Assembly.GetCallingAssembly();
			foreach (var item in registrations)
			{
				var component = item as IComponent;
				var classe = item as IClass;
				try
				{
					if(component!=null)
						Register(component, currentAssembly);
					else if (classe != null)
						Register(classe, currentAssembly);
				}
				catch (ContainerException)
				{
					throw;
				}
				catch (Exception exception)
				{
					throw new ContainerException("Exception registering " + component.Interface, exception);
				}
			}
		}

		public T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}
		public IEnumerable<T> ResolveAll<T>()
		{
			foreach (var item in ResolveAll(typeof(T)))
			{
				yield return (T)item;
			};
		}

		public abstract object Resolve(Type t);
		public abstract IEnumerable<object> ResolveAll(Type t);
		public abstract void Release(object t);

		public abstract object ContainerInstance { get; }
		protected abstract void Register(IComponent component, Assembly currentAssembly);
		protected abstract void Register(IClass component, Assembly currentAssembly);
	}
}