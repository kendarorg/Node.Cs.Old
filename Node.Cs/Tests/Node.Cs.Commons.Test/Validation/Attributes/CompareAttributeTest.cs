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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Attributes.Validation;

namespace Node.Cs.Commons.Test.Validation.Attributes
{
	/// <summary>
	/// Summary description for CompareAttributesTest
	/// </summary>
	[TestClass]
	public class CompareAttributeTest
	{
		[TestMethod]
		public void ItShouldBePossibleToSetTheWithField()
		{
			var sc = new CompareAttribute("FieldA");
			Assert.AreEqual("FieldA", sc.WithField);
		}

		[TestMethod]
		public void CompareValidStrings()
		{
			var sc = new CompareAttribute("FieldA");
			Assert.IsFalse(sc.IsValid("value", "Value"));
			Assert.IsTrue(sc.IsValid("value", "value"));
		}

		[TestMethod]
		public void CompareValueTypes()
		{
			var sc = new CompareAttribute("FieldA");
			Assert.IsFalse(sc.IsValid(12, 44));
			Assert.IsTrue(sc.IsValid(23, 23));
		}

		[TestMethod]
		public void CompareStructs()
		{
			var d1 = new DateTime(1000, 1, 1);
			var d2 = new DateTime(1000, 1, 1);
			var d3 = new DateTime(1001, 1, 1);
			var sc = new CompareAttribute("FieldA");
			Assert.IsFalse(sc.IsValid(d1, d3));
			Assert.IsTrue(sc.IsValid(d1, d2));
		}
	}
}
