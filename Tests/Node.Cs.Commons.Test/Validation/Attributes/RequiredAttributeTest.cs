using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Attributes.Validation;

namespace Node.Cs.Commons.Test.Validation.Attributes
{
	/// <summary>
	/// Summary description for RangeAttributesTest
	/// </summary>
	[TestClass]
	public class RequiredAttributeTest
	{

		[TestMethod]
		public void Validate()
		{
			var sc = new RequiredAttribute();
			Assert.IsFalse(sc.IsValid(null, null));
			Assert.IsFalse(sc.IsValid(string.Empty, typeof(string)));
			Assert.IsTrue(sc.IsValid("do@test.com", null));
			Assert.IsTrue(sc.IsValid(DateTime.Now, typeof(DateTime)));
		}
	}
}
