
using Http.Shared.Routing;

namespace IntranetWebApplication
{
	public class RouteConfig : IRouteInitializer
	{
		public void RegisterRoutes(IRoutingHandler routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
					name: "Default",
					url: "{controller}/{action}/{id}",
					defaults: new { controller = "Home", action = "Index", id = RoutingParameter.Optional }
			);
		}
	}
}