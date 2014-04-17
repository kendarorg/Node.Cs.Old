using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.OnReceive;

namespace Node.Cs.Razor
{
	public class RazorContextManager : ContextManager
	{
		private readonly NodeCsContext _context;

		public RazorContextManager(IListenerContainer listener)
			: base(listener)
		{
		}

		public RazorContextManager(NodeCsContext context)
			: base(null)
		{
			_context = context;
		}

		protected override HttpContextBase AssignContext(HttpContextBase context)
		{
			return _context;
		}
	}
}
