namespace Node.Cs.Lib.Controllers
{
	public class NotFoundResponse : HttpCodeResponse
	{
		public string Url { get; private set; }
		public NotFoundResponse(string url)
			: base(404)
		{
			Url = url;
		}
	}
}