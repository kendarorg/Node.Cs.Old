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


using System.Linq;

namespace Http.Shared
{
	public static class UrlUtils
	{
		public static string CleanUpUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return string.Empty;
			return url.Trim('/');
		}

		public static string Combine(params string[] urls)
		{
			var result = string.Empty;
			for (int index = 0; index < urls.Length; index++)
			{
				var url = urls[index];
				if (!string.IsNullOrEmpty(url))
				{
					url = url.Replace("\\", "/").Trim('/');
					if (!string.IsNullOrEmpty(url))
					{
						result += "/" + url;
					}
				}
			}
			return result;
		}

		public static string GetDirectoryName(string requestPath)
		{
			var result = string.Empty;
			var splitted = requestPath.Split('/');
			for (int index = 0; index < splitted.Length - 1; index++)
			{
				result += "/" + splitted[index];
			}
			return Combine(result);
		}

		public static string GetFileName(string requestPath)
		{
			var splitted = requestPath.Split('/');
			if (!splitted.Any())
			{
				splitted = requestPath.Split('\\');
			}
			return splitted.Last();
		}

		internal static string GetExtension(string path)
		{
			var fileName = GetFileName(path);
			var lastDot = fileName.Split('.');
			if (lastDot.Length <= 1) return null;
			return lastDot.Last();
		}
	}
}
