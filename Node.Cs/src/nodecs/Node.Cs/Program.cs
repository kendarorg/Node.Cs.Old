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


using GenericHelpers;
using System;
using System.Reflection;
using System.ServiceProcess;

namespace NodeCs
{
	class Program
	{
		static void Main(string[] args)
		{
			var helpMessage = ResourceContentLoader.LoadText("Help.txt", Assembly.GetExecutingAssembly());
			var clp = new CommandLineParser(args, helpMessage);
			var runner = new NodeCsService();
			
			if (clp.Has("service"))
			{
				if (clp.Has("serviceSHOULDSTART REALLY"))
				{
					var servicesToRun = new ServiceBase[] { };
					ServiceBase.Run(servicesToRun);
				}
				else { throw new NotImplementedException(); }
			}
			else
			{
				runner.Run(args,helpMessage);
				while (true)
				{
					Shared.NodeRoot.Write();
					var result = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(result))
					{
						if (result.Trim().ToLowerInvariant() == "close")
						{
							break;
						}
						else
						{
							if (!runner.Execute(result))
							{
								break;
							}
						}
					}
				}


				Shared.NodeRoot.WriteLine("Stopping.");
				runner.Terminate();
				Shared.NodeRoot.WriteLine("Stopped.");
			}
		}
	}
}
