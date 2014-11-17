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
using NodeCs.Shared.Attributes.Validation;

namespace Http.Test
{
	[TestClass]
	public class ValidationAttributesTest
	{
		#region StringLengthAttribute
		[TestMethod]
		public void StringLengthAttribute_validating_zero_max_length()
		{
			var an = new StringLengthAttribute(0);
			Assert.AreEqual(0, an.MinimumLength);
			Assert.IsTrue(an.IsValid(null, typeof(string)));
			Assert.IsTrue(an.IsValid(string.Empty, typeof(string)));
			Assert.IsFalse(an.IsValid("valid", typeof(string)));
		}

		[TestMethod]
		public void StringLengthAttribute_valid_with_length_non_zero()
		{
			var an = new StringLengthAttribute(5);
			Assert.AreEqual(0, an.MinimumLength);
			Assert.IsTrue(an.IsValid(null, typeof(string)));
			Assert.IsTrue(an.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(an.IsValid("valid", typeof(string)));
			Assert.IsFalse(an.IsValid("invalid", typeof(string)));
		}


		[TestMethod]
		public void StringLengthAttribute_valid_with_min_length_not_zero()
		{

			var an = new StringLengthAttribute(5)
			{
				MinimumLength = 1
			};
			Assert.AreEqual(1, an.MinimumLength);
			Assert.IsFalse(an.IsValid(null, typeof(string)));
			Assert.IsFalse(an.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(an.IsValid("valid", typeof(string)));
			Assert.IsFalse(an.IsValid("invalid", typeof(string)));
		}
		#endregion

		#region RangeAttribute
		[TestMethod]
		public void RangeAttribute_validating_zero_min_length()
		{
			var an = new RangeAttribute(0);
			Assert.AreEqual(0, an.Min);
			Assert.IsTrue(an.IsValid(1, null));
			Assert.IsFalse(an.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(an.IsValid("0", typeof(string)));
			Assert.IsTrue(an.IsValid(Int64.MaxValue, typeof(Int64)));
		}

		[TestMethod]
		public void RangeAttribute_valid_with_length_non_zero()
		{
			var an = new RangeAttribute(5);
			Assert.AreEqual(5, an.Min);
			Assert.IsFalse(an.IsValid(null, typeof(string)));
			Assert.IsFalse(an.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(an.IsValid("44", typeof(string)));
			Assert.IsFalse(an.IsValid("invalid", typeof(string)));
		}


		[TestMethod]
		public void RangeAttribute_valid_with_min_length_not_zero()
		{

			var an = new RangeAttribute(1, 5);
			Assert.AreEqual(1, an.Min);
			Assert.AreEqual(5, an.Max);
			Assert.IsFalse(an.IsValid(null, typeof(string)));
			Assert.IsFalse(an.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(an.IsValid("4", typeof(string)));
			Assert.IsFalse(an.IsValid("28", typeof(string)));
		}
		#endregion
	}
}
