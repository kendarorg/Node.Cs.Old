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


using System.Collections.Generic;
using System.IO;
using System.Text;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.PathProviders
{
	public class FileSystemPathProvider : IPathProvider
	{
		private FileSystemWatcher _watcher;
		private IGlobalPathProvider _parentPathProvider;

		public string Root { get; private set; }

		public void Initialize(string connectionString, IGlobalPathProvider parentPathProvider)
		{
			_parentPathProvider = parentPathProvider;
			Root = connectionString;
			if (!Path.IsPathRooted(Root))
			{
				Root = Path.Combine(NodeCsServer.RootDir.TrimEnd(new[] { '\\', '/' }), Root.TrimStart(new[] { '\\', '/' }));
			}
			ConnectionString = connectionString;
			_watcher = new FileSystemWatcher(Root);
			_watcher.IncludeSubdirectories = true;
			_watcher.Changed += OnFileChanged;
			_watcher.EnableRaisingEvents = true;
		}

		public string ConnectionString { get; private set; }

		public string GetFileNamed(string relativePathWithoutExtension)
		{
			var fileName = Path.GetFileName(relativePathWithoutExtension);
			var directory = Path.GetDirectoryName(relativePathWithoutExtension);
			if (string.IsNullOrEmpty(directory)) directory = string.Empty;
			if (directory == "\\") directory = string.Empty;
			var realDirectory = Path.Combine(Root, directory);

			var result = Directory.GetFiles(realDirectory, fileName + ".*");
			if (result.Length == 0) return null;
			return directory + "/" + Path.GetFileName(result[0]);
		}

		public bool FileExists(string relativePath)
		{
			var realPath = Path.Combine(Root, relativePath);
			return File.Exists(realPath);
		}

		public bool DirectoryExists(string relativePath)
		{
			var realPath = Path.Combine(Root, relativePath);
			return Directory.Exists(realPath);
		}

		public IEnumerable<Step> ReadBinary(string relativePath)
		{
			var realPath = Path.Combine(Root.TrimEnd(new[] { '/', '\\' }), relativePath.TrimStart(new[] { '/', '\\' }));

			using (FileStream sourceStream = File.Open(realPath, FileMode.Open, FileAccess.Read))
			{
				var destinationStream = new MemoryStream();
				yield return Coroutine.InvokeTaskAndWait(sourceStream.CopyToAsync(destinationStream));
				destinationStream.Seek(0, SeekOrigin.Begin);
				byte[] buffer = destinationStream.ToArray();
				destinationStream.Close();
				yield return Step.DataStep(buffer);
			}
		}

		public IEnumerable<Step> ReadText(string relativePath)
		{
			var resultBytes = new Container();
			yield return Coroutine.InvokeLocalAndWait(() => ReadBinary(relativePath), resultBytes);

			var data = (byte[])resultBytes.RawData;

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
			return _parentPathProvider.IsFileChanged(relativePath);
		}

		void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Changed)
			{
				if (File.Exists(e.FullPath))
				{
					var fullPath = e.FullPath;
					var realPath = fullPath.Substring(Root.Length);
					_parentPathProvider.HandleFileChanged(realPath);
				}
			}
		}
	}
}
