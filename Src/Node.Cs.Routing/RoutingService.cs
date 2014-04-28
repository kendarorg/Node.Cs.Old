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
using System.Text;

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
			var parKeys = new List<string>(pars.Keys);
			for (int i = 0; i < _routeDefinitions.Count; i++)
			{
				var routeDefinition = _routeDefinitions[i];
				var weigth = IsMatching(routeDefinition, pars, parKeys);
				mr.Add(new MatchingRoute { Definition = routeDefinition, Weight = weigth });
			}
			if (mr.Count == 0) return null;
			var max = int.MinValue;
			var maxRoute = -1;
			var hashRoute = new HashSet<string>();
			var routeSplitted = new StringBuilder();
			var routeMissing = new StringBuilder();
			for (int index = 0; index < mr.Count; index++)
			{
				var match = mr[index];
				if (match.Weight > max)
				{
					var route = CreateRoute(match, pars, parKeys, hashRoute, routeSplitted, routeMissing);
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

		private int IsMatching(RouteDefinition routeDefinition, Dictionary<string, object> pars, List<string> keys)
		{
			int weight = 0;
			for (int i = 0; i < keys.Count; i++)
			{
				var key = keys[i];
				if (!pars.ContainsKey(key))
				{
					var pardef = routeDefinition.Parameters[key];
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



		private string CreateRoute(MatchingRoute match, Dictionary<string, object> pars,
			List<string> parsKeys, HashSet<string> routeParamsUsed, StringBuilder routeSplitted,
			StringBuilder routeMissing)
		{
			routeMissing.Clear();
			routeSplitted.Clear();
			routeParamsUsed.Clear();
			routeSplitted.Clear();
			var routeDefinition = match.Definition;

			for (int i = 0; i < routeDefinition.Url.Count; i++)
			{
				var par = routeDefinition.Url[i];
				var name = par.LowerRoute;
				if (!par.IsParameter)
				{
					routeSplitted.Append("/");
					routeSplitted.Append(name);
				}
				else
				{
					if (pars.ContainsKey(name))
					{
						routeSplitted.Append("/");
						routeSplitted.Append(pars[par.Name].ToString());
						routeParamsUsed.Add(name);
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
			routeMissing.Clear();
			var hasRoute = false;
			for (var i = 0; i < parsKeys.Count; i++)
			{
				var parKey = parsKeys[i];
				if (!routeParamsUsed.Contains(parKey))
				{
					if (hasRoute) routeMissing.Append("&");
					routeMissing.Append(HttpUtility.UrlEncode(parKey))
						.Append("=")
						.Append(HttpUtility.UrlEncode(pars[parKey].ToString()));
					hasRoute = true;
				}
			}
			//var url = "/" + string.Join("/", routeSplitted);
			if (routeMissing.Length > 0)
			{
				routeSplitted.Append("?");
				routeSplitted.Append(routeMissing);
			}
			return routeSplitted.ToString();
		}
	}
}
