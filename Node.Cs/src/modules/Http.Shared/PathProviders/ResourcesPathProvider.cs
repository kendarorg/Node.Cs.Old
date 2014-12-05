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



using CoroutinesLib.Shared;
using GenericHelpers;
using Http.Shared.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Http.Shared.PathProviders
{
	public class ResourcesPathProvider:IPathProvider
	{
		private readonly HashSet<string> _resources;
		private readonly Dictionary<string, byte[]> _files;
		private readonly HashSet<string> _dirs;
		private Assembly _asm;

		public ResourcesPathProvider(Assembly asm = null)
		{
			if (asm == null) asm = Assembly.GetCallingAssembly();
			_resources = new HashSet<string>(asm.GetManifestResourceNames(),StringComparer.OrdinalIgnoreCase);
			_files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			_dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_asm = asm;
		}

		public void RegisterPath(string resourceId, string path)
		{
			path = path.Replace('/', Path.DirectorySeparatorChar);
			path = path.Trim(Path.DirectorySeparatorChar);
			var resourceName = _resources.FirstOrDefault(r => r.EndsWith(resourceId, StringComparison.OrdinalIgnoreCase));
			if(resourceName==null)
			{
				throw new FileLoadException(resourceId);
			}
			var content = ResourceContentLoader.LoadBytes(resourceName, _asm);
			_files.Add(path,content);
			var splitted = path.Split(Path.DirectorySeparatorChar);
			var entry = string.Empty;
			for (int i = 0; i < (splitted.Length-2); i++)
			{
				entry += Path.DirectorySeparatorChar + splitted[i];
				entry = entry.Trim(Path.DirectorySeparatorChar);
				if (!_dirs.Contains(entry))
				{
					_dirs.Add(entry);
				}
			}
		}


		public IEnumerable<string> FindFiles(string dir)
		{
			return new List<string>();
		}

		public IEnumerable<string> FindDirs(string dir)
		{
			return new List<string>();
		}

		public bool Exists(string relativePath,out bool isDir)
		{
			relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
			relativePath = relativePath.Trim(Path.DirectorySeparatorChar);
			if (_dirs.Contains(relativePath))
			{
				isDir = true;
				return true;
			}
			isDir = false;
			return _files.ContainsKey(relativePath);
		}

		public IEnumerable<ICoroutineResult> GetStream(string relativePath, IHttpContext context)
		{
			relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
			relativePath = relativePath.Trim(Path.DirectorySeparatorChar);
			yield return CoroutineResult.Return(new StreamResult(DateTime.UtcNow,_files[relativePath]));
		}

		public void ShowDirectoryContent(bool showDirectoryContent)
		{
			
		}
	}
}
