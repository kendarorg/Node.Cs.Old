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
using ConcurrencyHelpers.Containers.Asyncs;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.OnReceive
{
	public interface IPagesManager
	{
		PageDescriptor GetFilePath(string localPath);
		bool IsSessionCapable(string localPath);
	}

	public class PagesManager : IPagesManager
	{
		public PagesManager()
		{

		}

		public PageDescriptor GetFilePath(string localPath)
		{
			return GetFilePathInternal(localPath);
		}

		public bool IsSessionCapable(string localPath)
		{
			return IsSessionCapableInternal(localPath);
		}

		private static AsyncLockFreeDictionary<string, PageDescriptor> Pages = new AsyncLockFreeDictionary<string, PageDescriptor>(new Dictionary<string, PageDescriptor>());

		private static PageDescriptor GetFilePathInternal(string localPath)
		{
			localPath = PathCleanser.WebToFilePath(localPath);
			if (Pages.ContainsKey(localPath))
			{
				return Pages[localPath];
			}
			var de = GlobalVars.PathProvider.DirectoryExists(localPath);
			if (de)
			{
				var rpp = GlobalVars.PathProvider.GetProviderForFileNamed(localPath + "/index");
				var fn = rpp.GetFileNamed(localPath + "/index");
				var pd = new PageDescriptor
				{
					PathProvider = rpp,
					RealPath = fn
				};
				Pages.Add(localPath, pd);
				return pd;
			}
			var pp = GlobalVars.PathProvider.GetProviderForFile(localPath);
			if (pp != null)
			{
				var pd = new PageDescriptor
				{
					PathProvider = pp,
					RealPath = localPath
				};
				Pages.Add(localPath, pd);
				return pd;
			}
			return null;
		}

		private static bool IsSessionCapableInternal(string localPath)
		{
			string foundedExt = null;
			if (!Path.HasExtension(localPath))
			{
				foreach (var ext in GlobalVars.ExtensionHandler.Extensions)
				{
					var newPath = localPath + ext;
					var tmpReal = GetFilePathInternal(newPath);
					if (tmpReal != null)
					{
						localPath += ext;
						foundedExt = ext;
						break;
					}
				}
			}
			if (foundedExt == null) return false;
			var foundedPath = GetFilePathInternal(localPath);
			if (foundedPath == null) return false;
			return GlobalVars.ExtensionHandler.IsSessionCapable(foundedExt);
		}
	}
}
