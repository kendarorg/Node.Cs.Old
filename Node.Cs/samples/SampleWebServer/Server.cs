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
using NodeCs.Shared;

namespace SampleWebServer
{

	public class RunHttpModule
	{
		public void Execute()
		{
			const string logFile = "SampleLog.txt";
			File.Delete(logFile);
			File.WriteAllText(logFile, "");
			NodeRoot.SetLoggingLevel("all");
			NodeRoot.SetLogFile(logFile);

			//Load curl to ease testing
			NodeRoot.LoadModule("curl");

			//Load Node caching
			var cacheModule = NodeRoot.LoadNamedModule("mainCache", "node.caching");

			//Load static path provider
			var staticPathProvider = NodeRoot.LoadNamedModule("root", "http.pathprovider.staticcontent");
			//Set the root where files will be taken
			staticPathProvider.SetParameter("connectionString", InferWebRootDir());
			staticPathProvider.SetParameter("cache", cacheModule);

			//Load markdown renderer
			var markdownModule = NodeRoot.LoadModule("http.renderer.markdown");
			markdownModule.SetParameter("cache", cacheModule);

			//Load cshtml renderer
			var razorModule = NodeRoot.LoadModule("http.renderer.razor");
			razorModule.SetParameter("cache", cacheModule);

			//Load routing engine
			NodeRoot.LoadModule("http.routing");
			NodeRoot.LoadModule("http.mvc");


			//Load the webserver
			NodeRoot.LoadModule("http");

			//Load the application dll
			NodeRoot.LoadDll("SampleWebServer");

			//Initialize application server
			var http = NodeRoot.GetModule("http");
			http.SetParameter("port", 8881);
			http.SetParameter("virtualDir", "nodecs");
			http.SetParameter("host", "localhost");
			http.SetParameter("showDirectoryContent", false);

			
			//Start everthing
			NodeRoot.StartModules();
		}

		private static string InferWebRootDir()
		{
			var rootDir = NodeRoot.Main.AssemblyDirectory;
			//Search all root directories
			while (!Directory.Exists(Path.Combine(rootDir, "web")))
			{
				rootDir = Path.GetDirectoryName(rootDir);
			}
			rootDir = Path.Combine(rootDir, "web");
			return rootDir;
		}
	}
}
