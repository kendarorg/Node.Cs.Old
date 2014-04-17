using System;

namespace Node.Cs.Lib.Controllers
{
	public interface IResponseHandlersFactory
	{
		void Register<T>(IResponseHandler responseHandler);
		IResponseHandler Load(Type type);
	}
}
