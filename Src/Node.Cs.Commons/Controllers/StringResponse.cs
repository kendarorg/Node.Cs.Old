using System.Text;

namespace Node.Cs.Lib.Controllers
{
	public class StringResponse : DataResponse
	{

		public virtual void Initialize(string data, string contentType = null, Encoding contentEncoding = null)
		{
			ContentType = contentType ?? "text/plain";
			ContentEncoding = contentEncoding ?? Encoding.UTF8;
			Data = ContentEncoding.GetBytes(data);
		}
	}
}