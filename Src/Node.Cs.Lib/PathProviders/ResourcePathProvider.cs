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
using System.Text;
using System.Threading;
using ConcurrencyHelpers.Coroutines;
using GenericHelpers;

namespace Node.Cs.Lib.PathProviders
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class ResourceWebAttribute : Attribute
	{
		public string ResourceName { get; private set; }
		public string RealPath { get; private set; }

		public ResourceWebAttribute(string realPath, string resourceName)
		{
			ResourceName = resourceName;
			if (!realPath.StartsWith("/")) throw new ArgumentException("Resource Paths must start with /. The value " + realPath + " was wrong!");
			RealPath = realPath.Replace("/", "\\").Trim('\\');

		}
	}
	public class ResourcePathProvider : IPathProvider
	{
		private Dictionary<string, byte[]> _dataFiles;
		private HashSet<string> _directories;
		private IGlobalPathProvider _parentPathProvider;
		private Assembly _assembly;

		public void Initialize(string connectionString, IGlobalPathProvider parentPathProvider)
		{
			ConnectionString = connectionString;
			_dataFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			_directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_parentPathProvider = parentPathProvider;
			_assembly = AppDomain.CurrentDomain.GetAssemblies()
				.First(a => a.GetName().Name.IndexOf(connectionString, StringComparison.OrdinalIgnoreCase) >= 0);

			foreach (ResourceWebAttribute attr in _assembly.GetCustomAttributes(typeof(ResourceWebAttribute), false))
			{
				BuildDataFile(attr);
			}
		}

		private void BuildDataFile(ResourceWebAttribute attr)
		{
			var bytesData = ResourceContentLoader.LoadBytes(attr.ResourceName, _assembly);
			var path = attr.RealPath.ToLowerInvariant();
			_dataFiles.Add(path, bytesData);
			var splittedPath = Path.GetDirectoryName(path).Split('\\');
			for (int i = 1; i <= splittedPath.Length; i++)
			{
				var tempPath = ("\\" + string.Join("\\", splittedPath, 0, i)).Replace("\\\\", "\\");
				_directories.Add(tempPath);
			}
		}

		public string ConnectionString { get; private set; }

		public string GetFileNamed(string relativePathWithoutExtension)
		{
			if (!FileExists(relativePathWithoutExtension)) return null;
			return relativePathWithoutExtension.Replace("/", "\\").Trim('\\');
		}

		public bool FileExists(string relativePath)
		{
			relativePath = relativePath.Replace("/", "\\").Trim('\\');
			return _dataFiles.ContainsKey(relativePath);
		}

		public bool DirectoryExists(string relativePath)
		{
			relativePath = relativePath.Replace("/", "\\").Trim('\\');
			return _directories.Contains(relativePath);
		}

		public IEnumerable<Step> ReadBinary(string relativePath)
		{
			relativePath = relativePath.Replace("/", "\\").Trim('\\');
			var data = _dataFiles[relativePath];
			yield return Step.DataStep(data);
		}

		public IEnumerable<Step> ReadText(string relativePath)
		{
			relativePath = relativePath.Replace("/", "\\").Trim('\\');
			var data = _dataFiles[relativePath];
			var result = Encoding.UTF8.GetString(data);
			string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			var preamble = Encoding.UTF8.GetPreamble();
			if (data.Length > preamble.Length)
			{
				if (data[0] == preamble[0] && data[1] == preamble[1] && data[2] == preamble[2])
				{
					yield return Step.DataStep(result.Remove(0, byteOrderMarkUtf8.Length));
				}
				else
				{
					yield return Step.DataStep(result);
				}
			}
			else
			{
				yield return Step.DataStep(result);
			}
		}

		public bool IsFileChanged(string relativePath)
		{
			return false;
		}
	}
}
