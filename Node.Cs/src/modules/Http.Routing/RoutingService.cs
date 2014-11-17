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


using GenericHelpers;
using Http.Shared.Contexts;
using Http.Shared.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Http.Routing
{
	//TODO: This must be optimized grouping definitions
	public class RoutingService : IRoutingHandler
	{
		public List<RouteDefinition> _routeDefinitions;
		private string _virtualDir;

		public RoutingService()
		{
			_routeDefinitions = new List<RouteDefinition>();
		}

		public void AddStaticRoute(string route)
		{
			if (!route.StartsWith("~/"))
			{
				throw new Exception(string.Format("Invalid route '{0}' not starting with ~", route));
			}
			route = route.TrimStart('~');
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}
			_routeDefinitions.Add(new RouteDefinition(route.ToLowerInvariant(), true, false));
		}

		public RouteInstance Resolve(string route, IHttpContext context)
		{
			if (route.StartsWith("~/"))
			{
				route = route.TrimStart('~');
			}
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}

			var lower = route.ToLowerInvariant();
			var splitted = route.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			for (int index = (_routeDefinitions.Count - 1); index >= 0; index--)
			{
				var routeDefinition = _routeDefinitions[index];
				var routeInstance = IsValid(splitted, routeDefinition, lower);
				if (routeInstance != null)
				{
					if (routeInstance.StaticRoute || routeInstance.BlockRoute)
					{
						return routeInstance;
					}
					if (routeInstance.Parameters.ContainsKey("controller"))
					{
						var key = routeInstance.Parameters["controller"].ToString();
						if (!_controllers.ContainsKey(key))
						{
							key = key + "controller";
						}
						if (!_controllers.ContainsKey(key))
						{
							continue;
						}
						routeInstance.Controller = _controllers[key];
					}
					return routeInstance;
				}
			}
			return null;
		}

		public void SetVirtualDir(string virtualDir)
		{
			_virtualDir = virtualDir;
		}

		private RouteInstance IsValid(string[] route, RouteDefinition routeDefinition, string lower)
		{
			if (routeDefinition.IsStatic)
			{
				if (lower.StartsWith(routeDefinition.RouteString))
				{
					return new RouteInstance(true, false);
				}
			}

			if (route.Length > routeDefinition.Url.Count)
			{
				var last = routeDefinition.Url[routeDefinition.Url.Count - 1];
				if (!last.IsParameter && !last.LowerRoute.StartsWith("*", StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
			}

			var routeInstance = new RouteInstance();
			var index = -1;
			while (index < (route.Length - 1))
			{
				index++;
				
				var routeValue = route[index];
				if (index >= routeDefinition.Url.Count)
				{
					return null;
				}
				var block = routeDefinition.Url[index];
				if (block.IsParameter)
				{
					if (block.LowerRoute.StartsWith("*", StringComparison.OrdinalIgnoreCase))
					{
						var paramName = block.LowerRoute.Substring(1);
						var paramValue = "/" + string.Join("/", route, index, route.Length - index).Trim('/');
						routeInstance.Parameters.Add(paramName, paramValue);
						index = route.Length;
					}
					else
					{
						routeInstance.Parameters.Add(block.Name, routeValue);
					}
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
				throw new Exception(string.Format("Invalid route '{0}' not starting with ~", route));
			}
			route = route.TrimStart('~');
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}

			var routeDefinition = new RouteDefinition(route, ReflectionUtils.ObjectToDictionary(parameters));
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



		private Dictionary<string, Type> _controllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		public void LoadControllers(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				_controllers.Add(type.Name, type);
			}
			for (int i = 0; i < _routeDefinitions.Count; i++)
			{
				var routeDefinition = _routeDefinitions[i];
				if (routeDefinition.Parameters != null && routeDefinition.Parameters.ContainsKey("controller"))
				{
					var controllerParameter = routeDefinition.Parameters["controller"];
					if (controllerParameter.Value != null && !string.IsNullOrWhiteSpace(controllerParameter.Value.ToString()))
					{
						var controllerName = controllerParameter.Value.ToString();
						if (!_controllers.Any(c =>
							string.Equals(controllerName, c.Key, StringComparison.OrdinalIgnoreCase) ||
							string.Equals(controllerName + "Controller", c.Key, StringComparison.OrdinalIgnoreCase)))
						{
							throw new Exception("Missing controller " + controllerName);
						}
					}
				}
			}
		}

		public void IgnoreRoute(string route)
		{
			if (!route.StartsWith("~/"))
			{
				throw new Exception(string.Format("Invalid route '{0}' not starting with ~", route));
			}
			route = route.TrimStart('~');
			if (route.StartsWith("/"))
			{
				route = route.TrimStart('/');
			}

			var routeDefinition = new RouteDefinition(route, new Dictionary<string, object>(), false, true);
			CheckForConflicting(routeDefinition);
			_routeDefinitions.Add(routeDefinition);
		}
	}
}
