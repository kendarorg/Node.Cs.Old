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
	public class RegularExpressionAttributeTest
	{
		private const string EMAIL = @"^((([\w]+\.[\w]+)+)|([\w]+))@(([\w]+\.)+)([A-Za-z]{1,3})$";
		[TestMethod]
		public void ItShouldBePossibleToSetTheWithField()
		{
			var sc = new RegularExpressionAttribute(EMAIL);
			Assert.AreEqual(EMAIL, sc.Pattern);
		}

		[TestMethod]
		public void CompareValidStrings()
		{
			var sc = new RegularExpressionAttribute(EMAIL);
			Assert.IsFalse(sc.IsValid("dummy@creepy@foody", null));
			Assert.IsTrue(sc.IsValid("do@test.com", null));
		}
	}
}
