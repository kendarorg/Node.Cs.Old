using System.Text;
using Newtonsoft.Json;

namespace Node.Cs.Lib.Controllers
{
	public class JsonResponse : StringResponse
	{
		public virtual void Initialize(object data, string contentType = null, Encoding contentEncoding = null)
		{
			string stringData = "{}";
			ContentType = contentType ?? "application/json";
			if (data != null)
			{
				stringData = data as string;
				if (stringData == null)
				{
					stringData = JsonConvert.SerializeObject(data);
				}
			}
			base.Initialize(stringData, ContentType, contentEncoding);

		}
	}
}