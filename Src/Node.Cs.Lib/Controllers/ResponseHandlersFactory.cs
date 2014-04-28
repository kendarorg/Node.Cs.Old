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
using Node.Cs.Lib.Loggers;

namespace Node.Cs.Lib.Controllers
{
	public class ResponseHandlersFactory : IResponseHandlersFactory
	{
		public Dictionary<Type, IResponseHandler> _handlers = new Dictionary<Type, IResponseHandler>();

		public void Register<T>(IResponseHandler responseHandler)
		{
			if (_handlers.ContainsKey(typeof(T)))
			{
				Logger.Error("Duplicate handler '{0}' for type '{1}'", responseHandler.GetType(), typeof(T));
			}
			_handlers.Add(typeof(T), responseHandler);
		}

		public IResponseHandler Load(Type type)
		{
			if (!_handlers.ContainsKey(type))
			{
				if (typeof(HttpCodeResponse).IsAssignableFrom(type)) return null;
				if (typeof(DataResponse).IsAssignableFrom(type)) return null;
				Logger.Error("Missing handler for type '{0}'", type);
				return null;
			}
			return _handlers[type];
		}
	}
}
