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


using Http.Shared;
using Http.Shared.Authorization;
using Http.Shared.Routing;
using NodeCs.Shared;

namespace TestWebServer
{
	public class StartUp :
				IRouteInitializer, IFiltersInitializer, ILocatorInitialize,
				IAuthenticationDataProviderFactory
	{
		public void RegisterRoutes(IRoutingHandler handler)
		{
			handler.MapStaticRoute("~/static");
			handler.MapRoute("","~/catchall/{*allThatCanBe}",
				new
				{
					controller = "CatchAll",
					action = "CatchAll"
				});
			handler.MapRoute("", "~/{controller}/{action}/{id}",
				new
				{
					controller = "Home",
					action = "Index",
					id = RoutingParameter.Optional
				});
		}

		public void InitializeFilters(IFilterHandler handler)
		{

		}

		public void InitializeLocator(IServiceLocator serviceLocator)
		{

		}


		public IAuthenticationDataProvider CreateAuthenticationDataProvider()
		{
			return NullAuthenticationDataProvider.Instance;
		}
	}
}
