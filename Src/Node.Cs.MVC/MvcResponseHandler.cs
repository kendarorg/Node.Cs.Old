using System.Collections.Generic;
using Node.Cs.Lib.Contexts;
using Node.Cs.Lib.Controllers;

namespace Node.Cs.MVC
{
	public class MvcResponseHandler:IResponseHandler
	{
		public void Handle(IControllerWrapperInstance controller, INodeCsContext context, IResponse response)
		{
			var resultView = (ViewResponse) response;
			resultView.ModelState = controller.Instance.Get<ModelStateDictionary>("ModelState");
			resultView.ViewData = controller.Instance.Get<Dictionary<string, object>>("ViewData");
		}
	}
}
