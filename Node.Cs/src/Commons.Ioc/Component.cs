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

namespace Commons.Ioc
{
	public class Component
	{
		public static IIocWith<T> Create<T>()
		{
			return new Component<T>();
		}
	}
	public class Component<TInterface> :Component, IIocWith<TInterface>, IIocLifestyle, IComponent
	{
		internal Component()
		{
			LifeStyle = LifeStyle.Singleton;
			Interface = typeof(TInterface);
		}

		public Type Interface { get; private set; }
		public object Instance { get; private set; }
		public Type Implementation { get; private set; }
		public LifeStyle LifeStyle { get; private set; }
		public Func<IIoc, object> Factory { get; private set; }

		public IIocLifestyle WithInstance(TInterface item)
		{
			Instance = item;
			return this;
		}

		public IIocLifestyle With<T>()
		{
			Implementation = typeof(T);
			return this;
		}

		public IIocLifestyle WithFactory<T>(Func<IIoc, T> build)
		{
			Factory = (ioc => (object)build(ioc));
			return this;
		}

		public IFinalRegistration WithLifestyle(LifeStyle lifeStyle = LifeStyle.Singleton)
		{
			LifeStyle = lifeStyle;
			return this;
		}

		IFinalRegistration IIocLifestyle.WithLifestyle(LifeStyle lifeStyle)
		{
			return WithLifestyle(lifeStyle);
		}
	}
}