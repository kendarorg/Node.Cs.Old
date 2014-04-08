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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassWrapper;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Razor.Helpers
{
	public class OnRazorReceivedCoroutine : OnHttpListenerReceivedCoroutine
	{
		private readonly NodeCsContext _context;
		internal RazorHandler RazorResource;

		public OnRazorReceivedCoroutine(NodeCsContext context)
		{
			_context = context;
		}
		protected override bool IsChildRequest { get { return true; } }

		protected override Step InitializeContext(NodeCsContext context)
		{
			return Step.Current;
		}

		protected override NodeCsContext AssignContext(NodeCsContext context)
		{
			return _context;
		}


		protected override Step CallHandlerInstance(ICoroutine handlerInstance)
		{
			RazorResource = handlerInstance as RazorHandler;
			if (RazorResource != null)
			{
				RazorResource.IsChildCall = true;
			}
			return base.CallHandlerInstance(handlerInstance);
		}
	}
}
