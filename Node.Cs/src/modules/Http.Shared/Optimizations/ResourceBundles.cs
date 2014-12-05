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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Http.Shared.PathProviders;

namespace Http.Shared.Optimizations
{
	public class ResourceBundles : IResourceBundles
	{
		private readonly List<IPathProvider> _pathProviders;
		private readonly string _virtualDir;
		private readonly Dictionary<string, IBundle> _script = new Dictionary<string, IBundle>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, IBundle> _style = new Dictionary<string, IBundle>(StringComparer.InvariantCultureIgnoreCase);

		public ResourceBundles(string virtualDir, List<IPathProvider> pathProviders)
		{
			_pathProviders = pathProviders;
			_virtualDir = virtualDir.Trim('/');
		}

		public void Add(IBundle include)
		{
			var fisAdd = new List<string>();
			for (int index = 0; index < include.FisicalAddresses.Count; index++)
			{
				var item = include.FisicalAddresses[index].Trim('/');
				if (item.StartsWith("~"))
				{
					item = item.Trim('~', '/');
					item =  "/" + item;
				}
				if (item.Contains("{") || item.Contains("*"))
				{
					fisAdd.AddRange(FindRealPaths(item));
				}
				else
				{
					fisAdd.Add("/" + _virtualDir +item);
				}

			}
			var script = include as ScriptBundle;
			if (script != null)
			{
				_script[script.LogicalAddress] = new ScriptBundle(script.LogicalAddress, fisAdd);
			}
			else
			{
				var style = include as StyleBundle;
				if (style != null)
				{
					_style[style.LogicalAddress] = new StyleBundle(style.LogicalAddress, fisAdd);
				}
			}
		}

		private IEnumerable<string> FindRealPaths(string item)
		{
			var splittedItems = item.Split(new string[]{"/"},StringSplitOptions.RemoveEmptyEntries);
			var baseDir = string.Empty;
			foreach (var splittedItem in splittedItems)
			{
				if (splittedItem.IndexOf("*", StringComparison.OrdinalIgnoreCase) > 0 ||
					splittedItem.IndexOf("{version}", StringComparison.OrdinalIgnoreCase) > 0)
				{
					var re = splittedItem.Replace("*", "(.)*").Replace("{version}", "(.)*");
					var regex = new Regex(re);
					foreach (var pp in _pathProviders)
					{
						var ppr = pp;
						foreach (var sf in ppr.FindFiles(baseDir))
						{
							var splittedSf = sf.Split('/');
							var lastf = splittedSf.Last();
							if (regex.IsMatch(lastf))
							{
								yield return "/" + _virtualDir + baseDir+ "/"+lastf;
							}
						}
					}
				}
				else
				{
					baseDir = "/" + _virtualDir + baseDir + "/" + splittedItem;
				}
			}
		}

		public StileHelper GetStyles()
		{
			return new StileHelper(_style);
		}

		public ScriptHelper GetScripts()
		{
			return new ScriptHelper(_script);
		}
	}

	public abstract class BundleHelper
	{
		private readonly Dictionary<string, IBundle> _data;

		protected BundleHelper(Dictionary<string, IBundle> data)
		{
			_data = data;
		}

		public string Render(string index)
		{
			if (_data.ContainsKey(index))
			{
				return BuildData(_data[index]);
			}
			return string.Empty;
		}

		private string BuildData(IBundle bundle)
		{
			var sb = new StringBuilder();
			foreach (var item in bundle.FisicalAddresses)
			{
				AddItem(sb, item);
			}
			return sb.ToString();
		}

		protected abstract void AddItem(StringBuilder sb, string item);
	}

	public class StileHelper : BundleHelper
	{
		public StileHelper(Dictionary<string, IBundle> data)
			: base(data)
		{
		}

		protected override void AddItem(StringBuilder sb, string item)
		{
			sb.Append(string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\"></link>", item));
		}
	}

	public class ScriptHelper : BundleHelper
	{
		public ScriptHelper(Dictionary<string, IBundle> data)
			: base(data)
		{
		}

		protected override void AddItem(StringBuilder sb, string item)
		{
			sb.Append(string.Format("<script type=\"javascript\" src=\"{0}\"></script>", item));
		}
	}
}