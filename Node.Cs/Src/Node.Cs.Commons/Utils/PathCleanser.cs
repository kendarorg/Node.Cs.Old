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

namespace Node.Cs.Lib.Utils
{
	public class PathCleanser
	{
		public static String WebToFilePath(string path)
		{
			//To /path/to/file
			path = Cleanse(path).Substring(1);
			return path.Replace('/', Path.DirectorySeparatorChar).TrimStart('\\');
		}

		public static String Cleanse(string path)
		{
			path = path.Trim();
			if (path.StartsWith("~"))
			{
				path = path.Substring(1);
			}
			path = path.Trim(new[] {'/', '\\'});
			return "~/" + path;
		}

		public static String ReRoot(string path, string root)
		{
			if (Path.IsPathRooted(path)) return path;
			return Path.Combine(root.TrimEnd(new[] { '/', '\\' }), path.TrimStart(new[] { '/', '\\' }));
		}


		public static String ToStandardDirPath(string path)
		{
			path = path.Trim(new[] {'/', '\\'});
			return path.Replace('/', Path.DirectorySeparatorChar);
		}
	}
}
