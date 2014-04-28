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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Attributes.Validation;

namespace Node.Cs.Commons.Test.Validation.Attributes
{
	/// <summary>
	/// Summary description for RangeAttributesTest
	/// </summary>
	[TestClass]
	public class StringLengthAttributeTest
	{
		[TestMethod]
		public void ItShouldBePossibleToSetBothMinMax()
		{
			var sc = new StringLengthAttribute(3) {MinimumLength = 1};
			Assert.AreEqual(3, sc.MaximumLength);
		}


		[TestMethod]
		public void ItShouldBePossibleToSetMinOnly()
		{
			var sc = new StringLengthAttribute(3);
			Assert.AreEqual(3, sc.MaximumLength);
			Assert.AreEqual(0, sc.MinimumLength);
		}

		[TestMethod]
		public void Verify()
		{
			var sc = new StringLengthAttribute(3);
			Assert.IsTrue(sc.IsValid("12",null));
			Assert.IsTrue(sc.IsValid("", null));
			Assert.IsTrue(sc.IsValid("123", null));
			Assert.IsFalse(sc.IsValid("1234", null));
		}

		[TestMethod]
		public void VerifyWithMin()
		{
			var sc = new StringLengthAttribute(3) {MinimumLength = 2};
			Assert.IsTrue(sc.IsValid("12", null));
			Assert.IsFalse(sc.IsValid("", null));
			Assert.IsFalse(sc.IsValid("1", null));
			Assert.IsTrue(sc.IsValid("123", null));
			Assert.IsFalse(sc.IsValid("1234", null));
		}

	}
}
