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

using Commons.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Commons.CastleWindsor.Test
{
#if FUFFA
	[TestClass]
	public class CastleIocTest
	{
		[TestMethod]
		public void ItShouldBePossibleToInitializeTheWoleThing()
		{
			var ioc = CastleIoc.Instance;
			ioc.Register(
				Component.Create<IBaseTransient>()
						.With<BaseTransient>()
						.WithLifestyle(LifeStyle.Transient),
				Component.Create<IBaseSingleton>()
						.With<BaseSingleton>()
						.WithLifestyle(),
				Class.Create().FromAllAssembly()
						.BasedOn<IBaseAllAssemblies>()
						.WithName("AllAssemblies").WithLifestyle(LifeStyle.Transient),
				Component.Create<IUsingAll>()
						.With<UsingAll>()
						.WithLifestyle()
				);

			var base1 = ioc.Resolve<IBaseTransient>();
			var base2 = ioc.Resolve<IBaseTransient>();
			Assert.AreNotEqual((object) base1.Id, base2.Id);
			ioc.Release(base1);
			ioc.Release(base2);


			var singleton1 = ioc.Resolve<IBaseSingleton>();
			var singleton2 = ioc.Resolve<IBaseSingleton>();
			Assert.AreEqual((object) singleton1.Id, singleton2.Id);


			var all = ioc.Resolve<IUsingAll>();
			Assert.AreEqual(2,all.AllBase.Count());

		}
	}
#endif
}
