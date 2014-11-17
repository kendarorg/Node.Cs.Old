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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using GenericHelpers;
using System.Reflection;

namespace BuildUtils
{
	class Program
	{
		private static string _nugetExe;

		static void Main(string[] args)
		{
			var helpMessage = ResourceContentLoader.LoadText("Help.txt", Assembly.GetExecutingAssembly());
			var clp = new CommandLineParser(args, helpMessage);
			if (clp.Has("makenuspec"))
			{
				MakeNuspec(clp["makenuspec"]);
			}
			if (clp.Has("nuget"))
			{
				_nugetExe = clp["nuget"];
			}
			if (clp.Has("pack", "nuget", "nuspec", "sourceDir", "destDir", "dlls"))
			{
				BuildNuget(clp["nuspec"], clp["sourceDir"], clp["destDir"], clp["dlls"]);
			}
		}

		private static void BuildNuget(string nuspecFile, string sourceDir, string destDir, string dlls)
		{
			
		}

		private static void MakeNuspec(string csprojFile)
		{
			var csprojName = Path.GetFileNameWithoutExtension(csprojFile);
			var nuspecPath = Path.Combine(Path.GetDirectoryName(csprojFile), csprojName + ".nuspec");
			var document = XDocument.Load(csprojFile);
			var result = document.Descendants("PropertyGroup").FirstOrDefault(n => n.Descendants("PostBuildEvent").Any());
			if (result == null)
			{
				result = new XElement("PropertyGroup");
				result.Add(new XElement("PostBuildEvent"));
				document.Add(result);
			}
			var rewriteCsproj = true;
			var pbe = result.Descendants("PostBuildEvent").First();
			var value = pbe.Value;
			if (string.IsNullOrWhiteSpace(value))
			{
				pbe.Value = ResourceContentLoader.LoadText("nuspecCsproj.txt", Assembly.GetExecutingAssembly());
			}
			else if (value.ToLowerInvariant().IndexOf("nuget pack", 0, value.Length, StringComparison.InvariantCultureIgnoreCase) < 0)
			{
				pbe.Value += "\r\n" + ResourceContentLoader.LoadText("nuspecCsproj.txt", Assembly.GetExecutingAssembly());
			}
			else
			{
				rewriteCsproj = false;
			}

			if (rewriteCsproj)
			{
				using (var xtw = new XmlTextWriter(csprojFile, Encoding.UTF8))
				{
					xtw.Formatting = Formatting.Indented; // optional, if you want it to look nice
					document.WriteTo(xtw);
				}
			}
			if (!File.Exists(nuspecPath))
			{
				var template = ResourceContentLoader.LoadText("nuspecTemplate.xml", Assembly.GetExecutingAssembly());
				template = template.Replace("$(ProjectName)", csprojName);
				File.WriteAllText(csprojFile, template, Encoding.UTF8);
			}
		}
	}
}
