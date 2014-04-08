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
using System.Reflection;
using GenericHelpers;
using Node.Cs.Lib;

namespace Node.Cs.Cmd
{
	class Program
	{
		

		static void Main(string[] args)
		{
			var help = ResourceContentLoader.LoadText("Help.txt");
			var clp = new CommandLineParser(args, help);
			var executableCodeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
			if (clp.Has("h") || clp.Has("h"))
			{
				clp.ShowHelp();
				return;
			}
			NodeCsRunner.StartServer(args, executableCodeBase, help);
			Console.WriteLine("Type 'stop' to terminate.");
			Console.WriteLine("Type 'recycle' to recycle.");
			Console.WriteLine("Type 'cleancache' to reset the cache content.");
			while (true)
			{
				var line = Console.ReadLine();
				if (!string.IsNullOrWhiteSpace(line))
				{
					var data = line.Trim();
					if (data == "stop")
					{
						NodeCsRunner.StopServer();
						break;
					}
					else if (data == "recycle")
					{
						NodeCsRunner.Recycle();
					}
					else if (data == "cleancache")
					{
						NodeCsRunner.CleanCache();
					}
				}
				
			}
			
		}
	}
}
