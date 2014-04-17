using System;
using System.Linq;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Contexts;

namespace Node.Cs.Commons.Test
{
	[TestClass]
	public class ContextsTest
	{
		[TestMethod]
		public void FormCollectionTest()
		{
			var nvc = new NameValueCollection();
			nvc.Add("a","B");
			nvc.Add("C","d");
			var fc = new FormCollection(nvc);
			Assert.IsTrue(fc.AllKeys.Count(a => a == "a") == 1);
			Assert.IsTrue(fc.AllKeys.Count(a => a == "C") == 1);
			Assert.IsTrue(fc.AllKeys.Count(a => a == "c") == 0);
			Assert.IsTrue(fc.AllKeys.Count(a => a == "D") == 0);
		}
	}
}
