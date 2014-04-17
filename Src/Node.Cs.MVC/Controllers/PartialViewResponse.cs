namespace Node.Cs.Lib.Controllers
{
	public class PartialViewResponse : ViewResponse
	{
		public PartialViewResponse(string view, object model, object viewBag = null) :
			base(view, model, viewBag)
		{
		}
	}
}