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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassWrapper.Test
{
	public class SampleClass
	{
		public string StringProperty { get; set; }

		public void VoidMethod()
		{
			
		}

		public void ParamMethod(string val)
		{
			StringProperty = val;
		}

		public void ParamMethod(object val)
		{
			StringProperty = val.ToString();
		}
	}

	

	[TestClass]
	public class ClassWrapperTest
	{
		[TestMethod]
		[Ignore]
		public void WeShouldInvokeReturn()
		{


		}

		[TestMethod]
		[Ignore]
		public void WeShouldInvokeObject()
		{


		}

		[TestMethod]
		[Ignore]
		public void WeShouldTryInvoke()
		{


		}

		[TestMethod]
		public void ShouldBePossibleToCreateAWrapper()
		{
			var instance = new SampleClass();
			var classWrapperDescriptor = new ClassWrapperDescriptor(typeof(SampleClass));
			classWrapperDescriptor.Load();
			Assert.AreEqual(8, classWrapperDescriptor.Methods.Count);
			Assert.AreEqual(1, classWrapperDescriptor.Properties.Count);
			var classWrapper = classWrapperDescriptor.CreateWrapper(instance);
			classWrapper.Set("StringProperty","test");
			Assert.AreEqual("test",instance.StringProperty);
			var result = classWrapper.Get<string>("StringProperty");
			Assert.AreEqual("test", result);
		}


		[TestMethod]
		public void ShouldBePossibleToInvokeFunctionsOnWrapper()
		{
			var instance = new SampleClass();
			var classWrapperDescriptor = new ClassWrapperDescriptor(typeof(SampleClass));
			classWrapperDescriptor.Load();
			Assert.AreEqual(8, classWrapperDescriptor.Methods.Count);
			Assert.AreEqual(1, classWrapperDescriptor.Properties.Count);
			var classWrapper = classWrapperDescriptor.CreateWrapper(instance);
			var guid = Guid.NewGuid();
			classWrapper.Invoke("ParamMethod", guid);
			Assert.AreEqual(guid.ToString(), instance.StringProperty);
		}
	}
}
