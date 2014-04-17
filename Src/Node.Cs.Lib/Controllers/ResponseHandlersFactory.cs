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
				Logger.Error("Missing handler for type '{0}'", type);
				return null;
			}
			return _handlers[type];
		}
	}
}
