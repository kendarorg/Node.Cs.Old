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
using System.Diagnostics;
using System.IO;
using System.Text;
using CoroutinesLib.TestHelpers;
using Http.Contexts;
using Http.Shared.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.TestHelpers;

namespace Http.IntegrationTest
{
	public class BaseResponseHandlingTest
	{
		protected static string InferWebRootDir(string rootDir)
		{
			//Search all root directories
			while (!Directory.Exists(Path.Combine(rootDir, "web")))
			{
				rootDir = Path.GetDirectoryName(rootDir);
			}
			rootDir = Path.Combine(rootDir, "web");
			return rootDir;
		}

		protected static SimpleHttpContext CreateRequest(string uri)
		{
			var context = new SimpleHttpContext();
			var request = new SimpleHttpRequest();
			request.SetUrl(new Uri(uri));
			var response = new SimpleHttpResponse();
			var outputStream = new MockStream();
			response.SetOutputStream(outputStream);
			context.SetRequest(request);
			context.SetResponse(response);
			return context;
		}

		protected void VerifyContext(SimpleHttpContext context)
		{
			var outputStream = (MockStream)context.Response.OutputStream;
			Console.WriteLine(outputStream.WrittenBytes + " " + context.Request.Url + " " + outputStream.Sw.ElapsedMilliseconds);
			Console.WriteLine(" " + outputStream.Start + " " + outputStream.End);
			Assert.AreEqual(1, outputStream.ClosesCall);
			Assert.IsTrue(outputStream.WrittenBytes > 0);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var strings = Encoding.UTF8.GetString(bytes);
			Assert.IsTrue(strings.Length > 0);
			Assert.IsTrue(strings.IndexOf("Exception", StringComparison.Ordinal) < 0, strings);
		}

		protected IHttpContext PrepareRequest(string uri)
		{
			var context = CreateRequest(uri);
			var outputStream = (MockStream)context.Response.OutputStream;
			outputStream.Initialize();
			return context;
		}

		protected byte[] RunRequest(string uri, HttpModule http, RunnerForTest runner, int timeoutMs = 250)
		{
			var context = PrepareRequest(uri);
			http.ExecuteRequest(context);
			var outputStream = (MockStream)context.Response.OutputStream;

			var sw = new Stopwatch();
			sw.Start();
			while (outputStream.ClosesCall != 1 && sw.ElapsedMilliseconds < timeoutMs)
			{
				runner.RunCycleFor(timeoutMs);
			}
			Console.WriteLine(outputStream.WrittenBytes + " " + uri);
			Assert.AreEqual(1, outputStream.ClosesCall);
			Assert.IsTrue(outputStream.WrittenBytes > 0);
			var os = (MemoryStream)context.Response.OutputStream;
			os.Seek(0, SeekOrigin.Begin);
			var bytes = os.ToArray();
			var strings = Encoding.UTF8.GetString(bytes);
			Assert.IsTrue(strings.Length > 0);
			Assert.IsTrue(strings.IndexOf("Exception", StringComparison.Ordinal) < 0, strings);
			return bytes;
		}
	}
}
