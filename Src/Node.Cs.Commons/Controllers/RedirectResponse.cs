namespace Node.Cs.Lib.Controllers
{
	public class RedirectResponse : HttpCodeResponse
	{
		public string Url { get; private set; }
		public RedirectResponse(string url, bool permanent = false)
			: base(permanent ? 301 : 302)
		{
			Url = url;
		}
	}
}