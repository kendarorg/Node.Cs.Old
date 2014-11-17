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


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Moq;

namespace Http.Routing.Test
{
	class Utils
	{

		public static HttpContextBase MockContext(string url, Dictionary<string, string> pars = null, byte[] content = null, string httpMethod = "GET")
		{
			if (pars == null) pars = new Dictionary<string, string>();
			if (content == null) content = new byte[] { };

			var nvc = new NameValueCollection();
			
			foreach (var kvp in pars)
			{
				nvc.Add(kvp.Key, kvp.Value);
			}

			var requestStream = new MemoryStream(content);
			requestStream.Seek(0, SeekOrigin.Begin);
			var responseStream = new MemoryStream();
			var server = new Mock<HttpServerUtilityBase>(MockBehavior.Loose);
			var response = new Mock<HttpResponseBase>(MockBehavior.Strict);
			response.Setup(r => r.OutputStream).Returns(responseStream);

			var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
			request.Setup(r => r.UserHostAddress).Returns("127.0.0.1");
			request.Setup(r => r.Url).Returns(new Uri(url));
			request.Setup(r => r.ContentEncoding).Returns(Encoding.UTF8);
			request.Setup(r => r.Params).Returns(nvc);
			request.Setup(r => r.InputStream).Returns(requestStream);
			request.Setup(r => r.HttpMethod).Returns(httpMethod);

			var session = new Mock<HttpSessionStateBase>();
			session.Setup(s => s.SessionID).Returns(Guid.NewGuid().ToString());

			var context = new Mock<HttpContextBase>();
			context.SetupGet(c => c.Request).Returns(request.Object);
			context.SetupGet(c => c.Response).Returns(response.Object);
			context.SetupGet(c => c.Server).Returns(server.Object);
			context.SetupGet(c => c.Session).Returns(session.Object);
			return context.Object;
		}
	}
}
