namespace Node.Cs.Lib.Controllers
{
	public abstract class HttpCodeResponse : IResponse
	{
		public int HttpCode { get; private set; }
		protected HttpCodeResponse(int code)
		{
			HttpCode = code;
		}
	}
}