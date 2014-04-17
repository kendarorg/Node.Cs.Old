using System.Collections.Generic;

namespace Node.Cs.Lib.Controllers
{
	public interface IViewResponse
	{
		Dictionary<string, object> ViewData { get; set; }
		ModelStateDictionary ModelState { get; set; }
		object Model { get; set; }
		string View { get; set; }
		dynamic ViewBag { get; set; }
	}
}
