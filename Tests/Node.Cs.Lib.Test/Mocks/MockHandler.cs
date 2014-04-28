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
using System.Collections.Generic;
using System.Text;
using System.Web;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Test.Mocks
{
	public class MockHandler : Coroutine, IResourceHandler
	{

		public StringBuilder StringBuilder { get; set; }
		public void Initialize(HttpContextBase context, PageDescriptor filePath, CoroutineMemoryCache memoryCache,
			IGlobalExceptionManager globalExceptionManager, PathProviders.IGlobalPathProvider globalPathProvider, bool isChildRequest)
		{
			
		}

		public object Model { get; set; }

		public bool IsSessionCapable { get { return true; } }

		public dynamic ViewBag { get; set; }

		public Controllers.ModelStateDictionary ModelState { get; set; }

		public Dictionary<string, object> ViewData { get; set; }

		public override void OnError(Exception ex)
		{
			
		}

		public override IEnumerable<Step> Run()
		{
			yield break;
		}
	}
}
