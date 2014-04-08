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
using System.Linq;
using Node.Cs.Lib.Routing.RouteDefinitions;

namespace Node.Cs.Lib.Routing
{
	public sealed class RouteTree
	{
		public RouteTree()
		{
			RouteElement = string.Empty;
			//Action = string.Empty;
			//Controller = string.Empty;
			//View = null;
			Child = new List<RouteTree>();
		}

		public string RouteElement { get; set; }
		//public string Controller { get; set; }
		//public string Action { get; set; }
		public List<RouteTree> Child { get; private set; }
		//public string View { get; set; }
		public DynamicRoute RouteItem { get; private set; }



		public void AddChild(string pathWithParams, DynamicRoute dynamicRoute)
		{
			if(!string.IsNullOrWhiteSpace(dynamicRoute.Controller))dynamicRoute.Controller = dynamicRoute.Controller.ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(dynamicRoute.Action)) dynamicRoute.Action = dynamicRoute.Action.ToLowerInvariant();
			var splitted = pathWithParams.ToLowerInvariant().Split('/');
			if (splitted.Length == 0) return;
			var zero = splitted[0].ToLowerInvariant();
			if (zero == RouteElement && splitted.Length > 1)
			{
				var one = splitted[1];
				//Takes ""/"test"/"{param}
				var child = Child.FirstOrDefault(c => c.RouteElement == one);
				if (child == null)
				{
					child = new RouteTree { RouteElement = one };
					//add ""/"test"
					Child.Add(child);
					
				}
				//takes xxxx
				var subItem = string.Join("/", splitted.Skip(1).ToArray());
				child.AddChild(subItem, dynamicRoute);
			}
			else
			{
				if (RouteItem==null)
				{
					RouteItem = dynamicRoute;
				}
				else
				{
					//throw new DuplicateRouteException(dynamicRoute.Controller, dynamicRoute.Action);
					throw new Exception(); //TODO
				}
			}
		}
	}
}
