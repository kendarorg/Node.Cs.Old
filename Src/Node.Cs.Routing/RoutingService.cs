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
using System.Web;
using Node.Cs.Lib.Exceptions;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Routing
{
	//TODO: This must be optimized grouping definitions
	public class RoutingService : IRoutingService
	{
		public List<RouteDefinition> _routeDefinitions;
		private readonly List<string> _staticRoutes;

		public RoutingService()
		{
			_staticRoutes = new List<string>();
			_routeDefinitions = new List<RouteDefinition>();
		}

		public void AddStaticRoute(string route)
		{
			if (!route.StartsWith("~/"))
			{
				throw new NodeCsException("Invalid route '{0}' not starting with ~", route);
			}
			route = route.TrimStart('~');
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}
			_staticRoutes.Add(route.ToLowerInvariant());
		}

		public RouteInstance Resolve(string route, HttpContextBase context)
		{
			if (route.StartsWith("~/"))
			{
				route = route.TrimStart('~');
			}
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}
			foreach (var staticRoute in _staticRoutes)
			{
				if (route.StartsWith(staticRoute, StringComparison.OrdinalIgnoreCase))
				{
					return new RouteInstance(true);
				}
			}
			var splitted = route.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var routeDefinition in _routeDefinitions)
			{
				var routeInstance = IsValid(splitted, routeDefinition);
				if (routeInstance != null)
				{
					return routeInstance;
				}
			}
			return null;
		}

		private RouteInstance IsValid(string[] route, RouteDefinition routeDefinition)
		{
			if (route.Length > routeDefinition.Url.Count) return null;

			var routeInstance = new RouteInstance();
			var index = -1;
			while (index < (route.Length - 1))
			{
				index++;
				var routeValue = route[index];
				var block = routeDefinition.Url[index];
				if (block.IsParameter)
				{
					routeInstance.Parameters.Add(block.Name, routeValue);
				}
				else
				{
					if (string.Compare(routeValue, block.Name, StringComparison.OrdinalIgnoreCase) != 0) return null;
				}
			}
			while (index < (routeDefinition.Url.Count - 1))
			{
				index++;
				var block = routeDefinition.Url[index];
				if (block.IsParameter)
				{
					if (routeDefinition.Parameters.ContainsKey(block.Name))
					{
						var parameter = routeDefinition.Parameters[block.Name];
						if (parameter.Optional)
						{
							if (parameter.Value != null && parameter.Value.GetType() != typeof(RoutingParameter))
							{
								routeInstance.Parameters.Add(block.Name, parameter.Value);
							}
						}
						else if (parameter.Value != null)
						{
							routeInstance.Parameters.Add(block.Name, parameter.Value);
						}
						else
						{
							return null;
						}
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}

			}
			foreach (var param in routeDefinition.Parameters)
			{
				if (!routeInstance.Parameters.ContainsKey(param.Key))
				{
					var paramReal = param.Value;
					if (paramReal.Value != null && paramReal.Value.GetType() != typeof(RoutingParameter))
					{
						routeInstance.Parameters.Add(param.Key, paramReal.Value);
					}
				}
			}
			return routeInstance;
		}


		private void CheckForConflicting(RouteDefinition routeDefinition)
		{

		}


		public void AddRoute(string route, dynamic parameters = null)
		{
			if (!route.StartsWith("~/"))
			{
				throw new NodeCsException("Invalid route '{0}' not starting with ~", route);
			}
			route = route.TrimStart('~');
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}

			var routeDefinition = new RouteDefinition(route, NodeCsAssembliesManager.ObjectToDictionary(parameters));
			CheckForConflicting(routeDefinition);
			_routeDefinitions.Add(routeDefinition);
		}

		class MatchingRoute
		{
			public int Weight;
			public RouteDefinition Definition;
			public string Route;
		}

		public string ResolveFromParams(Dictionary<string, object> pars)
		{
			var mr = new List<MatchingRoute>();
			for (int i = 0; i < _routeDefinitions.Count; i++)
			{
				string tmpUrl;
				var routeDefinition = _routeDefinitions[i];
				var weigth = IsMatching(routeDefinition, pars);
				mr.Add(new MatchingRoute { Definition = routeDefinition, Weight = weigth });
			}
			if (mr.Count == 0) return null;
			var max = int.MinValue;
			var maxRoute = -1;
			for (int index = 0; index < mr.Count; index++)
			{
				var match = mr[index];
				if (match.Weight > max)
				{
					var route = CreateRoute(match, pars);
					if (route != null)
					{
						match.Route = route;
						maxRoute = index;
						max = match.Weight;
					}
				}
			}
			if (maxRoute != -1)
			{
				return mr[maxRoute].Route;
			}
			return null;
		}

		private int IsMatching(RouteDefinition routeDefinition, Dictionary<string, object> pars)
		{
			int weight = 0;
			var keys = routeDefinition.Parameters.Keys;
			foreach (var key in keys)
			{
				var pardef = routeDefinition.Parameters[key];
				if (!pars.ContainsKey(key))
				{
					if (!pardef.Optional)
					{
						return 0;
					}
					if (pardef.Value != null)
					{
						weight++;
					}
				}
				else
				{
					weight++;
				}
			}
			return weight;
		}



		private string CreateRoute(MatchingRoute match, Dictionary<string, object> pars)
		{
			var routeParamsUsed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var routeDefinition = match.Definition;
			var routeSplitted = new List<string>();

			foreach (var par in routeDefinition.Url)
			{
				if (!par.IsParameter)
				{
					routeSplitted.Add(par.Name);
				}
				else
				{
					if (pars.ContainsKey(par.Name))
					{
						routeParamsUsed.Add(par.Name);
						routeSplitted.Add(pars[par.Name].ToString());
					}
					else
					{
						var pdesc = routeDefinition.Parameters[par.Name];
						if (!pdesc.Optional)
						{
							return null;
						}
					}
				}
			}
			var routeMissing = new List<string>();
			foreach(var par in pars)
			{
				if (!routeParamsUsed.Contains(par.Key))
				{
					routeMissing.Add(string.Format("{0}={1}",HttpUtility.UrlEncode(par.Key),HttpUtility.UrlEncode(par.Value.ToString())));
				}
			}
			var url = "/" + string.Join("/", routeSplitted);
			if (routeMissing.Count > 0)
			{
				url += "?" + string.Join("&", routeMissing);
			}
			return url;
		}
	}
}
