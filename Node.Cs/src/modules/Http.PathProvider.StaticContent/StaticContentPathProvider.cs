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
using CoroutinesLib.Shared.Logging;
using Http.Contexts;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.PathProviders;
using NodeCs.Shared;
using NodeCs.Shared.Caching;
using System;
using System.Collections.Generic;
using System.IO;

namespace Http.PathProvider.StaticContent
{
	public class StaticContentPathProvider : IPathProvider, ILoggable
	{
		private const string STATIC_PATH_PROVIDER_CACHE_ID = "StaticContentPathProvider";
		private readonly string _root;
		private string _virtualDir;
		private ICacheEngine _cacheEngine;
		private FileSystemWatcher _watcher;
		private bool _showDirectoryContent;

		public StaticContentPathProvider(string root)
		{
			Log = ServiceLocator.Locator.Resolve<ILogger>();
			_showDirectoryContent = true;
			_root = root;
		}

		public bool Exists(string relativePath, out bool isDir)
		{
			isDir = false;
			relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
			relativePath = Path.Combine(_root, relativePath);
			if (File.Exists(relativePath))
			{
				return true;
			}
			if (Directory.Exists(relativePath))
			{
				isDir = true;
				return true;
			}
			return false;
		}


		public IEnumerable<ICoroutineResult> GetStream(string relativePath, IHttpContext context)
		{
			string requestPath;

			if (context.Request.Url.IsAbsoluteUri)
			{
				requestPath = context.Request.Url.LocalPath.Trim();
			}
			else
			{
				requestPath = context.Request.Url.ToString().Trim();
			}
			var parentPath = UrlUtils.GetDirectoryName(requestPath);
			relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
			var parent = UrlUtils.Combine(requestPath);
			relativePath = Path.Combine(_root, relativePath.TrimStart(Path.DirectorySeparatorChar));
			StreamResult result = null;
			if (_cacheEngine == null)
			{
				if (File.Exists(relativePath))
				{
					foreach (var item in LoadStreamContent(relativePath, true))
					{
						yield return item;
					}
				}
			}
			else
			{
				yield return _cacheEngine.AddAndGet(new CacheDefinition
				{
					Id = requestPath,
					LoadData = () => LoadStreamContent(relativePath),
					ExpireAfter = TimeSpan.FromSeconds(60)
				}, (a) =>
				{
					result = (StreamResult)a;
				}, STATIC_PATH_PROVIDER_CACHE_ID);
				if (result != null)
				{
					//context.Response.ContentType = MimeHelper.GetMime(relativePath);
					yield return CoroutineResult.Return(result);
				}
			}

			if (!Directory.Exists(relativePath) || _showDirectoryContent == false)
			{
				throw new HttpException(404, "File not found");
			}

			context.Response.ContentType = MimeHelper.GetMime("directory.html");
			var responseString = "<HTML><BODY>";

			responseString += "<table>";
			if (!string.IsNullOrEmpty(parentPath))
			{
				responseString += string.Format("<tr><td>&nbsp;</td><td><a href='{0}'>..</a></td></tr>", parentPath);
			}
			foreach (var dir in Directory.EnumerateDirectories(relativePath))
			{
				var dirName = Path.GetFileName(dir);

				responseString += string.Format("<tr><td>Dir</td><td><a href='{0}'>{0}</a></td></tr>", parent + "/" + dirName);
			}
			foreach (var dir in Directory.EnumerateFiles(relativePath))
			{
				var dirName = Path.GetFileName(dir);
				responseString += string.Format("<tr><td>File</td><td><a href='{0}'>{0}</a></td></tr>", parent + "/" + dirName);
			}
			responseString += "</table>";
			responseString += "</BODY></HTML>";
			yield return CoroutineResult.Return(
				new StreamResult(DateTime.UtcNow, System.Text.Encoding.UTF8.GetBytes(responseString)));
		}

		public void ShowDirectoryContent(bool showDirectoryContent)
		{
			_showDirectoryContent = showDirectoryContent;
		}

		private IEnumerable<ICoroutineResult> LoadStreamContent(string relativePath, bool fileExistsAlready = false)
		{
			if (fileExistsAlready || File.Exists(relativePath))
			{
				var resultMs = new MemoryStream();
				using (var sourceStream = new FileStream(relativePath,
					FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
				{
					var lastModification = File.GetLastWriteTimeUtc(relativePath);
					yield return CoroutineResult.RunTask(sourceStream.CopyToAsync(resultMs))
						.AndWait();
					sourceStream.Close();
					resultMs.Seek(0, SeekOrigin.Begin);
					var result = resultMs.ToArray();
					resultMs.Close();

					yield return CoroutineResult.Return(
						new StreamResult(lastModification, File.ReadAllBytes(relativePath)));
				}
			}
			else
			{
				Log.Debug("LoadStreamContent failed for {0}", Path.GetFileName(relativePath));
				yield return CoroutineResult.Return(null);
			}
		}

		public void SetVirtualDir(string virtualDir)
		{
			_virtualDir = virtualDir;
		}

		public void SetCachingEngine(ICacheEngine cacheEngine)
		{
			_cacheEngine = cacheEngine;
			_cacheEngine.AddGroup(new CacheGroupDefinition
			{
				ExpireAfter = TimeSpan.FromDays(1),
				Id = STATIC_PATH_PROVIDER_CACHE_ID,
				RollingExpiration = true
			});
		}

		public ILogger Log { get; set; }

		public void InitializeFileWatcher()
		{
			_watcher = new FileSystemWatcher(_root);
			_watcher.IncludeSubdirectories = true;
			_watcher.Changed += OnFileChanged;
			_watcher.EnableRaisingEvents = true;
		}

		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Changed)
			{
				if (_cacheEngine == null) return;
				if (File.Exists(e.FullPath))
				{
					var fullPath = e.FullPath;
					var realPath = "/" + _virtualDir + "/" + fullPath.Substring(_root.Length).Replace(Path.DirectorySeparatorChar, '/').Trim('/');
					_cacheEngine.InvalidateItem(realPath, STATIC_PATH_PROVIDER_CACHE_ID);
				}
			}
		}

		public IEnumerable<string> FindFiles(string dir)
		{
			var oriDir = dir.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

			dir = Path.Combine(_root, oriDir);
			if(!Directory.Exists(dir)) yield break;
			foreach (var file in Directory.GetFiles(dir))
			{
				var res = "/" + oriDir.Trim('/') + "/" + Path.GetFileName(file);
				yield return res;
					;
			}
		}

		public IEnumerable<string> FindDirs(string dir)
		{
			dir = dir.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
			dir = Path.Combine(_root, dir);
			if (!Directory.Exists(dir)) yield break;
			foreach (var file in Directory.GetFiles(dir)) yield return file;
		}
	}
}
