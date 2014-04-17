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
