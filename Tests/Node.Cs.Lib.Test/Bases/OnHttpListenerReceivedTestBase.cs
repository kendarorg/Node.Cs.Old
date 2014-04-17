using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.ForTest;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Settings;

namespace Node.Cs.Lib.Test.Bases
{
	public class OnHttpListenerReceivedTestBase
	{

		protected HttpRequestWrapper _request;
		protected Mock<IRoutingService> _routingService;
		protected Mock<ISessionStorage> _sessionStorage;
		protected Mock<IExtensionHandler> _extensionHandler;
		protected MockGlobalPathProvider _pathProvider;
		protected Mock<IListenerContainer> _container;
		protected Mock<INodeCsServer> _nodeCsServer;
		protected MemoryStream _memoryStream;
		protected HttpResponseWrapper _response;
		protected Mock<IControllersFactoryHandler> _controllersFactoryHandler;
		protected Mock<IResponseHandlersFactory> _responseHandler;


		protected void InitializeNodeCs(string url, string fileName)
		{
			_memoryStream = new MemoryStream();
			var tw = new StreamWriter(_memoryStream);
			_request = new HttpRequestWrapper(new HttpRequest(fileName, url, ""));
			_response = new HttpResponseWrapper(new HttpResponse(tw));
			_routingService = new Mock<IRoutingService>();
			_sessionStorage = new Mock<ISessionStorage>();
			_extensionHandler = new Mock<IExtensionHandler>();
			_pathProvider = new MockGlobalPathProvider();
			_container = new Mock<IListenerContainer>();
			_nodeCsServer = new Mock<INodeCsServer>();
			_responseHandler = new Mock<IResponseHandlersFactory>();
			_controllersFactoryHandler = new Mock<IControllersFactoryHandler>();

			GlobalVars.RoutingService = _routingService.Object;
			GlobalVars.SessionStorage = _sessionStorage.Object;
			GlobalVars.ExtensionHandler = _extensionHandler.Object;
			GlobalVars.PathProvider = _pathProvider;
			GlobalVars.Settings = NodeCsSettings.Defaults("C:");
			GlobalVars.GlobalFilters.Clear();
			GlobalVars.ControllersFactoryHandler = _controllersFactoryHandler.Object;
			GlobalVars.ResponseHandlers = _responseHandler.Object;

			_pathProvider.Files.Clear();
		}
	}
}
