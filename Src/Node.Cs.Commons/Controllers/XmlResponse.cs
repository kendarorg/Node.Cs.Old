using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Node.Cs.Lib.Controllers
{
	public class XmlResponse : StringResponse
	{
		public virtual void Initialize(object data, string contentType = null, Encoding contentEncoding = null)
		{
			string stringData = "";
			ContentType = contentType ?? "application/xml";
			if (data != null)
			{
				stringData = data as string;
				if (stringData == null)
				{
					var ns = new XmlSerializerNamespaces();
					ns.Add(string.Empty, string.Empty);
					var xss = new XmlSerializer(data.GetType());
					var sw = new StringWriter();
					xss.Serialize(sw, data, ns);
					stringData = sw.GetStringBuilder().ToString();
				}
			}
			base.Initialize(stringData, ContentType, contentEncoding);

		}
	}
}