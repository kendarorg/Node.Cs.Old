// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Node.Cs.Lib.Controllers
{
	public class XmlResponse : DataResponse
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
			ContentType = contentType ?? "text/plain";
			ContentEncoding = contentEncoding ?? Encoding.UTF8;
			Data = ContentEncoding.GetBytes(stringData);
		}
	}
}