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


using System.Diagnostics;
using Http.Shared.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeCs.Shared.Attributes;

namespace NodeCs.Test.Validation
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
			var an = new DataTypeAttribute(Shared.Attributes.DataType.DateTime);
			Assert.IsTrue(an.IsValid(new Stopwatch(), typeof(Stopwatch)));
			Assert.AreEqual(an.DataType, Shared.Attributes.DataType.DateTime);
		}

		[TestMethod]
		public void DisplayAttributeTest()
		{
			var an = new DisplayAttribute {Name = "test"};
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
