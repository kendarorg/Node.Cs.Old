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

namespace GenericHelpers.Test
{
	/// <summary>
	/// Summary description for AssembliesManagerTest
	/// </summary>
	[TestClass]
	public class AssembliesManagerTest
	{

		[TestMethod]
		public void ParsingAsimpleType()
		{
			const string tp = "System.String";
			var tb = AssembliesManager.ParseType(tp);
			Assert.IsNotNull(tb);
			Assert.AreEqual(tp, tb.ToString());
		}

		[TestMethod]
		public void LoadingAsimpleType()
		{
			const string tp = "System.String";
			var tb = AssembliesManager.LoadType(tp);
			Assert.AreEqual(typeof(string), tb);
		}

		[TestMethod]
		public void ParsingAsimpleGeneric()
		{
			const string tp = "System.Collections.Generic.IEnumerable<System.String>";
			var tb = AssembliesManager.ParseType(tp);
			Assert.IsNotNull(tb);
			Assert.AreEqual(tp, tb.ToString());
		}

		[TestMethod]
		public void LoadingAsimpleGeneric()
		{
			const string tp = "System.Collections.Generic.IEnumerable<System.String>";
			var tb = AssembliesManager.LoadType(tp);
			Assert.AreEqual(typeof(IEnumerable<string>), tb);
		}


		[TestMethod]
		public void ParsingAgenericWithChildren()
		{
			const string tp = "System.Collections.Generic.Dictionary<System.String,System.Int32>";
			var tb = AssembliesManager.ParseType(tp);
			Assert.IsNotNull(tb);
			Assert.AreEqual(tp, tb.ToString());
		}

		[TestMethod]
		public void LoadingAgenericWithChildren()
		{
			const string tp = "System.Collections.Generic.Dictionary<System.String,System.Int32>";
			var tb = AssembliesManager.LoadType(tp);
			Assert.AreEqual(typeof(Dictionary<string, int>), tb);
		}

		[TestMethod]
		public void ParsingAgenericWithComplex()
		{
			const string tp = "System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<System.Int32>>";
			var tb = AssembliesManager.ParseType(tp);
			Assert.IsNotNull(tb);
			Assert.AreEqual(tp, tb.ToString());
		}

		[TestMethod]
		public void LoadingAgenericWithComplex()
		{
			const string tp = "System.Collections.Generic.Dictionary<System.String,System.Collections.Generic.List<System.Int32>>";
			var tb = AssembliesManager.LoadType(tp);
			Assert.AreEqual(typeof(Dictionary<String, List<Int32>>), tb);
		}

		[TestMethod]
		public void ParsingAgenericWithComplexAndWhiteSpaces()
		{
			const string tp = "Dictionary <string ,List<int >> ";
			const string expect = "Dictionary<string,List<int>>";
			var tb = AssembliesManager.ParseType(tp);
			Assert.IsNotNull(tb);
			Assert.AreEqual(expect, tb.ToString());
		}

		[Ignore]
		[TestMethod]
		public void ShouldBeAbleToLoadTypesGivenTheirParent()
		{

		}

		[Ignore]
		[TestMethod]
		public void ShouldBeAbleToLoadTypesGivenTheirAttribute()
		{

		}
	}
}
