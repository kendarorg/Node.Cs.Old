using Node.Cs.Lib.Contexts;

namespace Node.Cs.Lib.Controllers
{
	public interface IResponseHandler
	{
		void Handle(IControllerWrapperInstance controller, INodeCsContext context, IResponse response);
	}
}
