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
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Commons.Ioc;
using Component = Castle.MicroKernel.Registration.Component;

namespace Commons.CastleWindsor
{
	public class CastleIoc : IocInstance
	{
		private readonly WindsorContainer _containerInstance;

		static CastleIoc()
		{
			SetInstance(new CastleIoc());
		}

		public static CastleIoc Instance { get { return (CastleIoc)GetInstance(); } }

		private CastleIoc()
		{
			_containerInstance = new WindsorContainer();
			_containerInstance.Kernel.Resolver.AddSubResolver(new ListResolver(_containerInstance.Kernel));
			_containerInstance.Kernel.Resolver.AddSubResolver(new ArrayResolver(_containerInstance.Kernel));
		}

		public override object Resolve(Type t)
		{
			return _containerInstance.Resolve(t);
		}

		public override IEnumerable<object> ResolveAll(Type t)
		{
			throw new Exception();
		}

		public override void Release(object t)
		{
			_containerInstance.Release(t);
		}

		public override object ContainerInstance
		{
			get { return _containerInstance; }
		}

		protected override void Register(IComponent component, Assembly currentAssembly)
		{
			var componentFor = Component.For(component.Interface);

			var withCreation = InitializeCreationPolicy(component, componentFor);
			var result = InitializeLifeStyle(component, withCreation);
			_containerInstance.Register(result);
		}

		protected override void Register(IClass component, Assembly currentAssembly)
		{
			var classes = InitializeAssemblies(component, currentAssembly);
			var basedOn = InitializeSelection(component, classes);
			var result = InitializeLifeStyle(component, basedOn);
			_containerInstance.Register(result);
		}

		private BasedOnDescriptor InitializeSelection(IClass classs, FromAssemblyDescriptor componentFor)
		{
			if (classs.Where != null)
			{
				return componentFor.Where(a=>classs.Where(a));
			}
			if (classs.BasedOn != null)
			{
				return componentFor.BasedOn(classs.BasedOn).WithServiceAllInterfaces();
			}
			throw new ContainerException("Implementation not allowed for component");
		}

		private FromAssemblyDescriptor InitializeAssemblies(IClass component,Assembly currentAssembly)
		{
			switch (component.FromAssembly)
			{
				case (FromAssembly.All):
					return Classes.FromAssemblyInThisApplication();
				case (FromAssembly.Current):
					return Classes.FromAssembly(currentAssembly);
				case(FromAssembly.Entry):
					return Classes.FromAssembly(Assembly.GetEntryAssembly());
				default:
					throw new ContainerException("Not supported type for assembly source "+component.FromAssembly);
			}
		}

		private BasedOnDescriptor InitializeLifeStyle(IClass component, BasedOnDescriptor componentFor)
		{
			switch (component.LifeStyle)
			{
				case (LifeStyle.Singleton):
					return componentFor.LifestyleSingleton();
				case (LifeStyle.Transient):
					return componentFor.LifestyleTransient();
				case (LifeStyle.WebRequest):
					return componentFor.LifestylePerWebRequest();
				case (LifeStyle.Pooled):
					return componentFor.LifestylePooled();
				default:
					throw new ContainerException("LifeStyle not allowed " + component.LifeStyle);
			}
		}

		private static ComponentRegistration<object> InitializeLifeStyle(IComponent component, ComponentRegistration<object> componentFor)
		{
			switch (component.LifeStyle)
			{
				case (LifeStyle.Singleton):
					return componentFor.LifestyleSingleton();
				case (LifeStyle.Transient):
					return componentFor.LifestyleTransient();
				case (LifeStyle.WebRequest):
					return componentFor.LifestylePerWebRequest();
				case (LifeStyle.Pooled):
					return componentFor.LifestylePooled();
				default:
					throw new ContainerException("LifeStyle not allowed " + component.LifeStyle);
			}
		}

		private ComponentRegistration<object> InitializeCreationPolicy(IComponent component, ComponentRegistration componentFor)
		{
			if (component.Instance != null)
			{
				return componentFor.Instance(component.Instance);
			}
			if (component.Implementation != null)
			{
				return componentFor.ImplementedBy(component.Implementation);
			}
			if (component.Factory != null)
			{
				return componentFor.UsingFactoryMethod(a => component.Factory(this));
			}
			throw new ContainerException("Implementation not allowed for interface " + component.Interface);
		}
	}
}
