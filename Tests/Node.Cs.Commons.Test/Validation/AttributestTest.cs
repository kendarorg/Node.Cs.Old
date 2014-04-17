using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Attributes;
using Node.Cs.Lib.Attributes.Validation;

namespace Node.Cs.Commons.Test.Validation
{
	/// <summary>
	/// Summary description for AttributestTest
	/// </summary>
	[TestClass]
	public class AttributestTest
	{

		[TestMethod]
		public void ActionNameAttribute()
		{
			var an = new ActionName("test");
			Assert.AreEqual(an.Name, "test");
		}

		[TestMethod]
		public void BindAttribute()
		{
			var an = new BindAttribute
			{
				Exclude = "test"
			};
			Assert.AreEqual(an.Exclude, "test");
		}

		[TestMethod]
		public void DataType()
		{
			var an = new DataTypeAttribute(Lib.Attributes.Validation.DataType.DateTime);
			Assert.IsTrue(an.IsValid(new Stopwatch(), typeof(Stopwatch)));
			Assert.AreEqual(an.DataType, Lib.Attributes.Validation.DataType.DateTime);
		}

		[TestMethod]
		public void DisplayAttributeTest()
		{
			var an = new DisplayAttribute();
			an.Name = "test";
			Assert.AreEqual(an.Name, "test");
		}

		[TestMethod]
		public void DisplayNameAttributeTest()
		{
			var an = new DisplayNameAttribute("test");
			Assert.AreEqual(an.Name, "test");
		}



		[TestMethod]
		public void HttpMethodsAttributes()
		{
			var de = new HttpDeleteAttribute("test");
			Assert.AreEqual(de.Action, "test");
			Assert.AreEqual(de.Verb, "DELETE");

			var g = new HttpGetAttribute("test");
			Assert.AreEqual(g.Action, "test");
			Assert.AreEqual(g.Verb, "GET");

			var pu = new HttpPutAttribute("test");
			Assert.AreEqual(pu.Action, "test");
			Assert.AreEqual(pu.Verb, "PUT");

			var p = new HttpPostAttribute("test");
			Assert.AreEqual(p.Action, "test");
			Assert.AreEqual(p.Verb, "POST");

			
			var r = new HttpRequestTypeAttribute("webDav","test");
			Assert.AreEqual(r.Action, "test");
			Assert.AreEqual(r.Verb, "WEBDAV");
		}

		[TestMethod]
		public void Scaffold()
		{
			var an = new ScaffoldColumnAttribute(true);
			Assert.IsTrue(an.Scaffold);

			an = new ScaffoldColumnAttribute(false);
			Assert.IsFalse(an.Scaffold);
		}
	}
}
