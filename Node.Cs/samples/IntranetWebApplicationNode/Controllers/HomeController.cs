using Http.Shared.Controllers;
using HttpMvc.Controllers;
using System.Collections.Generic;

namespace IntranetWebApplication.Controllers
{
	public class HomeController : ControllerBase
	{
		public IEnumerable<IResponse> Index()
		{
			ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

			yield return View();
		}

		public IEnumerable<IResponse> About()
		{
			ViewBag.Message = "Your app description page.";

			yield return View();
		}

		public IEnumerable<IResponse> Contact()
		{
			ViewBag.Message = "Your contact page.";

			yield return View();
		}
	}
}
