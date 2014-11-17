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
	public class Class : IClass, IFromAssembly, IIocLifestyle, IIocWhere
	{
		public static IFromAssembly Create()
		{
			return new Class();
		}

		private Class()
		{
			LifeStyle = LifeStyle.Singleton;
			FromAssembly = FromAssembly.All;
		}

		public Type BasedOn { get; private set; }
		public Func<Type, bool> Where { get; private set; }
		public FromAssembly FromAssembly { get; private set; }
		public LifeStyle LifeStyle { get; private set; }

		public IIocWhere FromCurrentAssembly()
		{
			FromAssembly = FromAssembly.Current;
			return this;
		}

		public IIocWhere FromAllAssembly()
		{
			FromAssembly = FromAssembly.All;
			return this;
		}

		public IIocWhere FromEntryAssembly()
		{
			FromAssembly = FromAssembly.Entry;
			return this;
		}

		public IFinalRegistration WithLifestyle(LifeStyle lifeStyle = LifeStyle.Singleton)
		{
			LifeStyle = lifeStyle;
			return this;
		}

		IIocWhere IIocWhere.Where(Func<Type, bool> func)
		{
			Where = func;
			return this;
		}

		IIocWhere IIocWhere.BasedOn<T>()
		{
			BasedOn = typeof (T);
			return this;
		}

		public IIocWhere WithName(string name, StringComparison comparison = StringComparison.InvariantCulture)
		{
			Where = (a) =>
			{
				return a.Name.IndexOf(name, comparison) >= 0;
			};
			return this;
		}
	}
}