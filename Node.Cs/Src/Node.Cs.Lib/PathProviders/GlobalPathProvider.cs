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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Settings;

namespace Node.Cs.Lib.PathProviders
{
	public class GlobalPathProvider : IGlobalPathProvider
	{
		private readonly IEnumerable<PathProviderDefinition> _pathProviderDefinitions;
		private ReadOnlyCollection<IPathProvider> _pathProviders;
		private readonly ConcurrentDictionary<string, string> _changedMemoryCache;

		public GlobalPathProvider()
		{
			_pathProviderDefinitions = GlobalVars.Settings.Paths.WebPaths;
			_changedMemoryCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public void Initialize(string connectionString, IGlobalPathProvider parentPathProvider)
		{
			var tmp = new List<IPathProvider>();
			foreach (var ppd in _pathProviderDefinitions)
			{
				var type = Type.GetType(ppd.ClassName);
				if (type == null) break;

				//This is called once, so no problem with activator
				var pathProvderInstance = (IPathProvider)Activator.CreateInstance(type);
				if (string.IsNullOrEmpty(ppd.FileSystemPath))
				{
					pathProvderInstance.Initialize(ppd.ConnectionString, this);
				}
				else
				{
					pathProvderInstance.Initialize(ppd.FileSystemPath, this);
				}
				tmp.Add(pathProvderInstance);
			}
			tmp.Reverse();
			_pathProviders = new ReadOnlyCollection<IPathProvider>(tmp);
		}

		public void LoadPathProvider(IPathProvider pathProvider)
		{
			var lof = new List<IPathProvider>(_pathProviders);
			lof.Add(pathProvider);
			_pathProviders = new ReadOnlyCollection<IPathProvider>(lof);
		}

		public string ConnectionString { get; private set; }

		public IPathProvider GetProviderForFileNamed(string relativePathWithoutExtension)
		{
			foreach (var pp in _pathProviders)
			{
				var result = pp.GetFileNamed(relativePathWithoutExtension);
				if (result != null) return pp;
			}
			return null;
		}

		public string GetFileNamed(string relativePathWithoutExtension)
		{
			var pp = GetProviderForFileNamed(relativePathWithoutExtension);
			if (pp == null) return null;
			return pp.GetFileNamed(relativePathWithoutExtension);
		}

		public IPathProvider GetProviderForFile(string relativePath)
		{
			foreach (var pp in _pathProviders)
			{
				var result = pp.FileExists(relativePath);
				if (result) return pp;
			}
			return null;
		}

		public bool FileExists(string relativePath)
		{
			var pp = GetProviderForFile(relativePath);
			return pp != null;
		}

		public IPathProvider GetProviderForDir(string relativePath)
		{
			foreach (var pp in _pathProviders)
			{
				var result = pp.DirectoryExists(relativePath);
				if (result) return pp;
			}
			return null;
		}

		public bool DirectoryExists(string relativePath)
		{
			var pp = GetProviderForDir(relativePath);
			return pp != null;
		}

		public IEnumerable<Step> ReadBinary(string relativePath)
		{
			IPathProvider goodPathProvider = null;
			foreach (var pp in _pathProviders)
			{
				var result = pp.FileExists(relativePath);
				if (result)
				{
					goodPathProvider = pp;
					break;
				}
			}
			if (goodPathProvider == null)
			{
				yield return Step.DataStep(new byte[] { });
			}
			else
			{
				var step = new Container();
				yield return Coroutine.InvokeLocalAndWait(() => goodPathProvider.ReadBinary(relativePath), step);
				yield return Step.DataStep(step.RawData);
			}
		}

		public IEnumerable<Step> ReadText(string relativePath)
		{
			IPathProvider goodPathProvider = null;
			foreach (var pp in _pathProviders)
			{
				var result = pp.FileExists(relativePath);
				if (result)
				{
					goodPathProvider = pp;
					break;
				}
			}
			if (goodPathProvider == null)
			{
				yield return Step.DataStep(string.Empty);
			}
			else
			{
				var step = new Container();
				yield return Coroutine.InvokeLocalAndWait(() => goodPathProvider.ReadText(relativePath), step);
				yield return Step.DataStep(step.RawData);
			}
		}

		public bool IsFileChanged(string relativePath)
		{
			return false;
			/*
			var result = _changedMemoryCache.ContainsKey(relativePath);
			if (result)
			{
				string outValue;
				_changedMemoryCache.TryRemove(relativePath, out outValue);
			}
			return result;*/
		}

		public void HandleFileChanged(string relativePath)
		{
			/*relativePath = relativePath.Replace("\\", "/");
			_changedMemoryCache.TryAdd(relativePath, relativePath);*/
		}
	}
}
