using System;
using Node.Cs.Lib;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Routing;

namespace Node.Cs.MVC
{
	public class NodeCsMvcInitializer : IPluginInitializer
	{
		public void Initialize(IRoutingService routingService)
		{
		}

		public void Initialize(IGlobalPathProvider globalPathProvider)
		{
		}


		public void Initialize(IResponseHandlersFactory responseHandlersFactory)
		{
			var responseHandler = new MvcResponseHandler();
			responseHandlersFactory.Register<ViewResponse>(responseHandler);
			responseHandlersFactory.Register<PartialViewResponse>(responseHandler);
		}
	}
}
