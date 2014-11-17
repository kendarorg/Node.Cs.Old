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


using Http;
using Http.Shared;
using Http.Shared.Routing;
using HttpMvc.Conversions;
using NodeCs.Shared;

namespace HttpMvc
{
	public sealed class MvcModule : NodeModuleBase
	{
		public MvcModule()
		{
			SetParameter("views","~/views/{controller}/{action}");
			SetParameter("scripts", "~/Scripts");
			SetParameter("content", "~/Content");
		}

		private DataResponseHandler _dataResponseHandler;
		private HttpModule _httpModule;

		public override void Initialize()
		{
			_httpModule = ServiceLocator.Locator.Resolve<HttpModule>(); ;

			_httpModule.AddConverter(new JsonNodeCsSerializer(),
				"application/json", "text/json", "text/javascript");

			_httpModule.AddConverter(new XmlNodeCsSerializer(),
				"application/xml", "application/atom+xml", "text/xslt", "text/xml", "text/xsl");

			_httpModule.AddConverter(new FormNodeCsSerializer(),
				"application/x-www-form-urlencoded", "multipart/form-data");
			_dataResponseHandler = new DataResponseHandler();
			_httpModule.RegisterResponseHandler(_dataResponseHandler);

			var routingModule = NodeRoot.GetModule("http.routing");
			var routingHandler = routingModule.GetParameter<IRoutingHandler>(HttpParameters.RoutingHandlerInstance);
			routingHandler.IgnoreRoute("~/");
			routingHandler.AddStaticRoute(GetParameter<string>("scripts"));
			routingHandler.AddStaticRoute(GetParameter<string>("content"));
			
			_httpModule.SetParameter(HttpParameters.HttpShowDirectoryContent,false);
		}

		protected override void Dispose(bool disposing)
		{
			if (_dataResponseHandler!=null)
			_httpModule.UnregisterResponseHandler(_dataResponseHandler);
		}
	}
}
