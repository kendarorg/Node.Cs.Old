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
	public class SampleClassStatic:SampleClass
	{
		public static string StaticStringProperty { get; set; }
		public static void StaticParamMethod(object val)
		{
			StaticStringProperty = val.ToString();
		}
	}
	[TestClass]
	public class StaticClassWrapperTest
	{
		[TestMethod]
		public void ShouldBePossibleToCreateStaticallyAWrapper()
		{
			var classWrapperDescriptor = new ClassWrapperDescriptor(typeof(SampleClassStatic));
			classWrapperDescriptor.Load();
			Assert.AreEqual(9, classWrapperDescriptor.Methods.Count);
			Assert.AreEqual(2, classWrapperDescriptor.Properties.Count);
			var classWrapper = classWrapperDescriptor.CreateWrapper();
			classWrapper.Set("StaticStringProperty", "test");
			Assert.AreEqual("test", SampleClassStatic.StaticStringProperty);
			var result = classWrapper.Get<string>("StaticStringProperty");
			Assert.AreEqual("test", result);
		}


		[TestMethod]
		public void ShouldBePossibleToInvokeStaticallyFunctionsOnWrapper()
		{
			var classWrapperDescriptor = new ClassWrapperDescriptor(typeof(SampleClassStatic));
			classWrapperDescriptor.Load();
			Assert.AreEqual(9, classWrapperDescriptor.Methods.Count);
			Assert.AreEqual(2, classWrapperDescriptor.Properties.Count);
			var classWrapper = classWrapperDescriptor.CreateWrapper();
			var guid = Guid.NewGuid();
			classWrapper.Invoke("StaticParamMethod", guid);
			Assert.AreEqual(guid.ToString(), SampleClassStatic.StaticStringProperty);
		}
	}
}
