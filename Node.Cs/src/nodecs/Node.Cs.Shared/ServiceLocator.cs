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
	public interface IServiceLocator
	{
		T Resolve<T>(bool nullIfNotFound = false);
		object Resolve(Type t,bool nullIfNotFound = false);
		void Release(object ob);
		void Register(Type t);
		void Register<T>(Func<Type,object> resolver, bool isSingleton = true);
		void Register<T>(object instance);
        void SetChildLocator(IServiceLocator childLocator);
	}

	public class ServiceLocator : IServiceLocator
	{
		private readonly static IServiceLocator _locator = new ServiceLocator();
		public static IServiceLocator Locator
		{
			get { return _locator; }
		} 
		class ServiceInstance
		{
			public bool IsSingleton;
			public object Instance;
			public Func<Type,object> Create;
		}
		private readonly Dictionary<Type, ServiceInstance> _services;
		private readonly Dictionary<Type, ServiceInstance> _resolved;

		public IServiceLocator ChildLocator { get; private set; }

        public void SetChildLocator(IServiceLocator childLocator)
        {
            if (ChildLocator != null)
            {
                return;
            }
            ChildLocator = childLocator;
        }

		private ServiceLocator()
		{
			_services = new Dictionary<Type, ServiceInstance>();
			_resolved = new Dictionary<Type, ServiceInstance>();
			Register<ILogger>(NullLogger.Create());
		}

		public void Register<T>(Func<Type,object> resolver = null,bool isSingleton=true)
		{
			if (resolver == null)
			{
				resolver = Activator.CreateInstance;
			}
			var instance = resolver(typeof(T));
			_services[typeof (T)] = new ServiceInstance
			{
				Create = resolver,
				IsSingleton = isSingleton,
				Instance = isSingleton ? instance : null
			};
			_resolved[instance.GetType()] = _services[typeof (T)];
			if (!isSingleton)
			{
				var disposable = instance as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public void Register(Type t)
		{
			if (ChildLocator != null)
			{
				ChildLocator.Register(t);
			}
			else
			{
				Func<Type,object> resolver = Activator.CreateInstance;

				_services[t] = new ServiceInstance
				{
					Create = resolver,
					IsSingleton = false,
					Instance = null
				};
				_resolved[t] = _services[t];
			}
		}

		public void Register<T>(object instance)
		{
			if (ChildLocator != null)
			{
				ChildLocator.Register<T>(instance);
			}
			else
			{
				_services[typeof (T)] = new ServiceInstance
				{
					Create = null,
					IsSingleton = true,
					Instance = instance
				};
				_resolved[instance.GetType()] = _services[typeof (T)];
			}
		}

		public T Resolve<T>(bool nullIfNotFound = false)
		{
			if (_services.ContainsKey(typeof (T)))
			{
				var desc = _services[typeof (T)];
				if (desc.Instance!=null) return (T) desc.Instance;
				desc.Instance = desc.Create(typeof(T));
				return (T)desc.Instance;
			}
			
			if (ChildLocator != null)
			{
				var result = ChildLocator.Resolve<T>();
				if(result != null) return  result;
			}
			if (typeof(T).IsAbstract || typeof(T).IsInterface || nullIfNotFound)
			{
				return default(T);
			}
			if (IsSystem(typeof (T))) return default(T);
			return Activator.CreateInstance<T>();
		}

		public object Resolve(Type t,bool nullIfNotFound = false)
		{
			if (_services.ContainsKey(t))
			{
				var desc = _services[t];
				if (desc.Instance != null) return desc.Instance;
				desc.Instance = desc.Create(t);
				return desc.Instance;
			}

			if (ChildLocator != null)
			{
				var result = ChildLocator.Resolve(t);
				if (result != null) return result;
			}

			if (t.IsAbstract || t.IsInterface || nullIfNotFound)
			{
				return null;
			}
			if (IsSystem(t)) return null;
			return Activator.CreateInstance(t);
		}

		private bool IsSystem(Type type)
		{
			var ns = type.Namespace ?? "";
			return type.IsPrimitive ||
						 ns.StartsWith("System.") ||
						 ns == "System." ||
			       type.Module.ScopeName == "CommonLanguageRuntimeLibrary";
		}


		public void Release(object ob)
		{
			if (ChildLocator != null)
			{
				ChildLocator.Release(ob);
			}
			if (_resolved.ContainsKey(ob.GetType()))
			{
				var desc = _resolved[ob.GetType()];
				if (desc.IsSingleton) return;
				var disposable = desc.Instance as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
