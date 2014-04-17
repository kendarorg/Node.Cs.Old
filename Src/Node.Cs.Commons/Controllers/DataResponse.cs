using System.Text;

namespace Node.Cs.Lib.Controllers
{
	public class DataResponse : IResponse
	{
		public byte[] Data { get; protected set; }
		public string ContentType { get; protected set; }
		public Encoding ContentEncoding { get; protected set; }
	}
}