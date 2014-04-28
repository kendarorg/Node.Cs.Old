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
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Test.Mocks
{
	public class OnHttpListenerReceivedCoroutineForTest : OnHttpListenerReceivedCoroutine
	{
		public int CallHandlerInstanceCalls = 0;
		public MockContext Ctx { get; set; }

		public OnHttpListenerReceivedCoroutineForTest(MockContext ctx)
		{
			//ReInitialize();
			Ctx = ctx;
		}

	/*	protected override HttpContextBase AssignContext(HttpContextBase context)
		{
			return Ctx;
		}

		protected override Step InitializeContext(HttpContextBase context)
		{
			return Step.Current;
		}*/

		protected Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			CallHandlerInstanceCalls++;
			return Step.Current;
		}

		public Action<Container> InvokeControllerAndWaitAction;

		protected Step InvokeControllerAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
		{
			InvokeControllerAndWaitAction(result);
			return Step.Current;
		}
	}
}