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


using Node.Cs.Lib.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node.Cs.Lib.Test.Mocks
{
	public class MockViewResponse : IViewResponse, IResponse
	{
		public MockViewResponse(string p)
		{
			// TODO: Complete member initialization
			View = p;
		}
		public Dictionary<string, object> ViewData { get; set; }
		public ModelStateDictionary ModelState { get; set; }
		public object Model { get; set; }
		public string View { get; set; }
		public dynamic ViewBag { get; set; }
	}

	public class TestCmController : ApiControllerBase
	{
		public TestCmController()
		{
			CalledAction = string.Empty;
			Data = new Dictionary<string, object>();
		}
		public static string CalledAction = string.Empty;
		public static Dictionary<string, object> Data = new Dictionary<string, object>();
		public IEnumerable<IResponse> TestCmAction()
		{
			CalledAction = "TestCmAction";
			yield return TextResponse("TestCmAction");
		}

		public IEnumerable<IResponse> TestHttpCode()
		{
			CalledAction = "TestHttpCode";
			yield return new RedirectResponse("FUFFA");
		}

		[MockFilterAtt]
		public IEnumerable<IResponse> TestViewResponse()
		{
			CalledAction = "TestViewResponse";
			yield return new MockViewResponse("/test/tset");
		}

		public IEnumerable<IResponse> TestDataResponse(string par1)
		{
			Data.Add("par1", par1);
			CalledAction = "TestDataResponse";
			yield return TextResponse("/test/tset");
		}

		public IEnumerable<IResponse> TestDataResponseIntString(string par1, int par2)
		{
			Data.Add("par1", par1);
			Data.Add("par2", par2);
			CalledAction = "TestDataResponseIntString";
			yield return TextResponse("/test/tset");
		}

		public IEnumerable<IResponse> TestDataResponseIntNullString(string par1, int? par2)
		{
			Data.Add("par1", par1);
			Data.Add("par2", par2);
			CalledAction = "TestDataResponseIntNullString";
			yield return TextResponse("/test/tset");
		}

		public IEnumerable<IResponse> TestDataResponseIntOptional(int par1=4)
		{
			Data.Add("par1", par1);
			CalledAction = "TestDataResponseIntOptional";
			yield return TextResponse("/test/tset");
		}
	}
}
