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
			var sc = new StringLengthAttribute(3);
			sc.MinimumLength = 1;
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
			var sc = new StringLengthAttribute(3);
			sc.MinimumLength = 2;
			Assert.IsTrue(sc.IsValid("12", null));
			Assert.IsFalse(sc.IsValid("", null));
			Assert.IsFalse(sc.IsValid("1", null));
			Assert.IsTrue(sc.IsValid("123", null));
			Assert.IsFalse(sc.IsValid("1234", null));
		}

	}
}
