using System.Collections.Generic;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Routing;

namespace Node.Cs.Lib
{
	public interface INodeCsServer
	{
		CoroutineThread NextCoroutine { get;  }
	}
}
