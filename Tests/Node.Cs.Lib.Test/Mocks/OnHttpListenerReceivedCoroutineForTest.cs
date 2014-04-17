using System;
using System.Collections.Generic;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Test
{
	public class OnHttpListenerReceivedCoroutineForTest : OnHttpListenerReceivedCoroutine
	{
		public int CallHandlerInstanceCalls = 0;
		public MockContext Ctx { get; set; }

		public OnHttpListenerReceivedCoroutineForTest(MockContext ctx)
		{
			ReInitialize();
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

		protected override Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			CallHandlerInstanceCalls++;
			return Step.Current;
		}

		public Action<Container> InvokeControllerAndWaitAction;

		protected override Step InvokeControllerAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
		{
			InvokeControllerAndWaitAction(result);
			return Step.Current;
		}
	}
}