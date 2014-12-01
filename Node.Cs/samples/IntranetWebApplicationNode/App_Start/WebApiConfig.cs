using Http.Shared.Routing;

namespace IntranetWebApplication
{
	public class WebApiConfig: IRouteInitializer
	{
		public void RegisterRoutes(IRoutingHandler routes)
		{
			routes.MapRoute(
					name: "DefaultApi",
					url: "api/{controller}/{id}",
					defaults: new { id = RoutingParameter.Optional });

			// Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
			// To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
			// For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
			routes.EnableQuerySupport();
		}
	}
}