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
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.PathProviders;

namespace Node.Cs.Lib.Test.Mocks
{
	public class MockGlobalPathProvider:IGlobalPathProvider
	{
		public Dictionary<string,string> Dirs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, string> Files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public void Initialize(string connectionString, IGlobalPathProvider parentPathProvider)
		{
			
		}

		public string ConnectionString { get; private set; }
		public string GetFileNamed(string relativePathWithoutExtension)
		{
			if (!FileExists(relativePathWithoutExtension)) return null;
			return Files[relativePathWithoutExtension];
		}

		public bool FileExists(string relativePath)
		{
			return Files.ContainsKey(relativePath);
		}

		public bool DirectoryExists(string relativePath)
		{
			return Dirs.ContainsKey(relativePath);
		}

		public IEnumerable<Step> ReadBinary(string relativePath)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Step> ReadText(string relativePath)
		{
			throw new NotImplementedException();
		}

		public bool IsFileChanged(string relativePath)
		{
			throw new NotImplementedException();
		}

		public void HandleFileChanged(string relativePath)
		{
			throw new NotImplementedException();
		}

		public void LoadPathProvider(IPathProvider pathProvider)
		{
			
		}

		public IPathProvider GetProviderForFile(string localPath)
		{
			if (!FileExists(localPath)) return null;
			return this;
		}

		public IPathProvider GetProviderForFileNamed(string localPath)
		{
			if (!FileExists(localPath)) return null;
			return this;
		}
	}
}
