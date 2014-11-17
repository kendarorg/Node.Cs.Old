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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NodeCs.Shared;

namespace Curl
{
	public class CurlModule : NodeModuleBase
	{
		public override void Initialize()
		{
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.RegisterCommand("curl.read", new CommandDefinition((Action<string, string>)CurlModule.Curl, typeof(string), typeof(string)));
			main.RegisterCommand("curl.read", new CommandDefinition((Action<string>)CurlModule.Curl, typeof(string)));
			main.RegisterCommand("curl.result", new CommandDefinition((Action<string, string>)CurlModule.Curlh, typeof(string), typeof(string)));
			main.RegisterCommand("curl.result", new CommandDefinition((Action<string>)CurlModule.Curlh, typeof(string)));
		}

		protected override void Dispose(bool disposing)
		{
			var main = ServiceLocator.Locator.Resolve<IMain>();
			main.UnregisterCommand("curl.read", 2);
			main.UnregisterCommand("curl.read", 1);
			main.UnregisterCommand("curl.result", 2);
			main.UnregisterCommand("curl.result", 1);
		}

		protected static void Curl(string url)
		{
			Curl("GET", url);
		}

		protected static void Curl(string verb, string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = verb;
			var response = (HttpWebResponse)request.GetResponse();
			NodeRoot.CWriteLine(string.Format("Retrieved '{0}' with verb '{1}':", url, verb));
			if (response.StatusCode != HttpStatusCode.OK)
			{
				NodeRoot.CWriteLine(string.Format("Status code '{0}'.", response.StatusCode));
			}
			var stream = request.GetResponse().GetResponseStream();
			if (stream == null)
			{
				NodeRoot.CWriteLine("No data");
				return;
			}
			var reader = new StreamReader(stream); ;
			string sLine = "";
			int i = 0;

			while (sLine != null)
			{
				i++;
				sLine = reader.ReadLine();
				if (sLine != null)
				{
					NodeRoot.CWriteLine(string.Format("{0}:{1}", NodeRoot.Lpad(i, 5), sLine));
				}
			}
		}

		protected static void Curlh(string url)
		{
			Curlh("GET", url);
		}

		protected static void Curlh(string verb, string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = verb;
			var response = (HttpWebResponse)request.GetResponse();
			NodeRoot.CWriteLine(string.Format("Retrieved '{0}' with verb '{1}':", url, verb));
			if (response.StatusCode != HttpStatusCode.OK)
			{
				NodeRoot.CWriteLine(string.Format("Status code '{0}'.", response.StatusCode));
			}
			var stream = request.GetResponse().GetResponseStream();
			if (stream == null)
			{
				NodeRoot.CWriteLine("No data");
				return;
			}
			var reader = new StreamReader(stream); ;
			string sLine = "";
			int i = 0;
			var length = 0;
			while (sLine != null)
			{
				i++;
				sLine = reader.ReadLine();
				if (sLine != null)
				{
					length += sLine.Length;
				}
			}
			NodeRoot.CWriteLine(string.Format("Read {0} bytes.", length));
		}
	}
}
