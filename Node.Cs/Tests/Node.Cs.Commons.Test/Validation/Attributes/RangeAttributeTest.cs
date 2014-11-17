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
	/// Summary description for RangeAttributesTest
	/// </summary>
	[TestClass]
	public class RangeAttributeTest
	{
		[TestMethod]
		public void ItShouldBePossibleToSetBothMinMax()
		{
			var sc = new RangeAttribute(10, 20);
			Assert.AreEqual(10, sc.Min);
			Assert.AreEqual(20, sc.Max);
		}


		[TestMethod]
		public void ItShouldBePossibleToSetMinOnly()
		{
			var sc = new RangeAttribute(10);
			Assert.AreEqual(10, sc.Min);
			Assert.AreEqual(null, sc.Max);
		}

		public void DoComapare<T>(object min, object max, object lower, object mid, object greater)
		{
			var type = typeof(T);
			var sc = new RangeAttribute((T)min);
			Assert.IsFalse(sc.IsValid((T)lower, type),type.ToString());
			Assert.IsTrue(sc.IsValid((T)mid, type), type.ToString());

			sc = new RangeAttribute((T)min, (T)max);
			Assert.IsFalse(sc.IsValid((T)lower, type), type.ToString());
			Assert.IsTrue(sc.IsValid((T)mid, type), type.ToString());
			Assert.IsFalse(sc.IsValid((T)greater, type), type.ToString());
		}

		[TestMethod]
		public void CompareInt32()
		{
			DoComapare<Int32>(10, 40, 1, 30, 100);
		}

		[TestMethod]
		public void CompareDecimal()
		{
			DoComapare<Decimal>((Decimal)10, (Decimal)40, (Decimal)1, (Decimal)30, (Decimal)100);
		}

		[TestMethod]
		public void CompareInt64()
		{
			DoComapare<Int64>((Int64)10, (Int64)40, (Int64)1, (Int64)30, (Int64)100);
		}

		[TestMethod]
		public void CompareDouble()
		{
			DoComapare<Double>(10.0, 40.0, 1.0, 30.0, 100.0);
		}

		[TestMethod]
		public void CompareSingle()
		{
			DoComapare<Single>((Single)10.0, (Single)40.0, (Single)1.0, (Single)30.0, (Single)100.0);
		}
	}
}
