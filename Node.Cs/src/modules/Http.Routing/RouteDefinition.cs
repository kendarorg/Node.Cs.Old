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


using Http.Shared.Routing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Http.Routing
{
	public class RouteDefinition
	{
		public string RouteString { get; private set; }
		public RouteDefinition(string route, Dictionary<string, object> objectToDictionary,bool isStatic=false,bool isBlock=false)
		{
			IsStatic = isStatic;
			IsBlock = isBlock;
			RouteString = route.ToLowerInvariant();
			Parameters = new Dictionary<string, ParameterDefintion>(StringComparer.OrdinalIgnoreCase);
			foreach (var item in objectToDictionary)
			{
				if (item.Value.GetType() == typeof(RoutingParameter))
				{
					Parameters.Add(item.Key, new ParameterDefintion { Name = item.Key, Optional = true });
				}
				else
				{
					Parameters.Add(item.Key, new ParameterDefintion { Name = item.Key, Value = item.Value });
				}
			}
			var splitted = route.Split('/');
			var resultUrl = new List<UrlItemDescriptor>();
			foreach (var block in splitted)
			{
				if (block.StartsWith("{") && block.EndsWith("}"))
				{
					resultUrl.Add(new UrlItemDescriptor
					{
						IsParameter = true,
						Name = block.TrimStart('{').TrimEnd('}'),
						LowerRoute = block.TrimStart('{').TrimEnd('}').ToLowerInvariant()
					});
				}
				else
				{
					resultUrl.Add(new UrlItemDescriptor
					{
						Name = block.TrimStart('{').TrimEnd('}'),
						LowerRoute = block.TrimStart('{').TrimEnd('}').ToLowerInvariant()
					});
				}
			}
			IsStatic = false;
			IsBlock = false;
			Url = new ReadOnlyCollection<UrlItemDescriptor>(resultUrl);
		}

		public bool IsBlock { get; private set; }
		public bool IsStatic { get; private set; }

		public RouteDefinition(string route, bool isStatic, bool isBlock)
		{
			RouteString = route.ToLowerInvariant();
			var splitted = route.Split('/');
			var resultUrl = new List<UrlItemDescriptor>();
			foreach (var block in splitted)
			{
				resultUrl.Add(new UrlItemDescriptor
					{
						Name = block,
						LowerRoute = block.ToLowerInvariant(),
						IsParameter = false
					});
			}
			Url = new ReadOnlyCollection<UrlItemDescriptor>(resultUrl);
			IsStatic = isStatic;
			IsBlock = isBlock;
		}

		public ReadOnlyCollection<UrlItemDescriptor> Url { get; private set; }
		public Dictionary<string, ParameterDefintion> Parameters { get; private set; }
	}


}
