using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.PathProviders;

namespace Node.Cs.Lib.Test
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
