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
using System.Reflection;
using ClassWrapper;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib
{
	public partial class NodeCsServer
	{

		private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string strTempAssmbPath = null;

			foreach (var path in GlobalVars.Settings.Paths.BinPaths)
			{
				strTempAssmbPath = path;
				if (!strTempAssmbPath.EndsWith("\\")) strTempAssmbPath += "\\";
				// ReSharper disable once StringIndexOfIsCultureSpecific.1
				if (args.Name.IndexOf(",") > 0)
				{
					// ReSharper disable once StringIndexOfIsCultureSpecific.1
					strTempAssmbPath += args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
				}
				else
				{
					strTempAssmbPath += args.Name + ".dll";
				}

				if (File.Exists(strTempAssmbPath)) break;
				strTempAssmbPath = null;
			}

			if (strTempAssmbPath != null)
			{
				return Assembly.LoadFile(strTempAssmbPath);
			}
			if (GlobalVars.Settings.Debugging.DebugAssemblyLoading)
			{
				Logger.Warn("Unable to load assembly {0}", args.Name);
			}

			return null;
		}

		private void LoadApplication(string applicationDll, IEnumerable<string> list, string rootDir)
		{
			var tempBinPaths = new List<string>();
			tempBinPaths.Add(rootDir);
			tempBinPaths.AddRange(list);

			for (var i = 0; i < tempBinPaths.Count; i++)
			{
				var binPath = tempBinPaths[i];
				var realPath = Path.Combine(binPath, applicationDll + ".dll");

				if (File.Exists(realPath))
				{
					var bytes = File.ReadAllBytes(realPath);
					var preexistingAsm = NodeCsAssembliesManager.GetIfExists(applicationDll + ".dll");
					var asm = preexistingAsm ?? Assembly.LoadFile(realPath);
					var nodeCsInitializerType = asm.GetTypes().FirstOrDefault(t => t.Name == "GlobalNodeCs");
					if (nodeCsInitializerType == null)
					{
						nodeCsInitializerType = typeof(FakeInitializer);
					}
					var wrapper = new ClassWrapperDescriptor(nodeCsInitializerType);
					wrapper.Load();
					var initializerInstance = Activator.CreateInstance(nodeCsInitializerType);
					_globalInitializer = wrapper.CreateWrapper(initializerInstance);

					GlobalVars.ApplicationLocation = asm.Location;
					return;
				}
			}
			throw new NodeCsException("Application dll '{0}.dll' not found on App_Bins or root directory!", applicationDll);
		}
	}
}
