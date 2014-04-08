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
using System.Web;
using Node.Cs.Lib.Routing.RouteDefinitions;

namespace Node.Cs.Lib.Routing
{
	public class RoutingService : IRoutingService
	{
		private static string SanitizePath(string pathWithParams)
		{
			if (!pathWithParams.StartsWith("/")) pathWithParams = "/" + pathWithParams;
			pathWithParams = pathWithParams.TrimEnd('/');
			return pathWithParams.ToLowerInvariant();
		}

		private readonly Dictionary<string, ViewDescriptor> _views;
		private readonly Dictionary<string, string> _controllers;
		private readonly RouteTree _root;
		public List<KeyValuePair<string, StaticRoute>> _staticRoutes;

		public RoutingService()
		{
			_views = new Dictionary<string, ViewDescriptor>(StringComparer.InvariantCultureIgnoreCase);
			_controllers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			_root = new RouteTree();
			_staticRoutes = new List<KeyValuePair<string, StaticRoute>>();
		}


		public Route GetRoute(string pathWithParams)
		{
			pathWithParams = SanitizePath(pathWithParams);
			var staticRoute = FindStaticRoute(pathWithParams);
			if (staticRoute != null)
			{
				return staticRoute;
			}
			var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			return FindRoute(_root, pathWithParams, dict);
		}

		private Route FindStaticRoute(string pathWithParams)
		{
			for (int i = 0; i < _staticRoutes.Count; i++)
			{
				var kvp = _staticRoutes[i];
				if (pathWithParams.StartsWith(kvp.Key))
				{
					var res = pathWithParams.Substring(kvp.Key.Length);

					return new Route(kvp.Value, null, kvp.Value.Destination + res);
				}
			}
			return null;
		}

		private Route FindRoute(RouteTree node, string pathWithParams, Dictionary<string, string> dict)
		{
			Route result = null;
			var splitted = pathWithParams.Split('/');
			if (splitted.Length == 0) return null;
			var zero = splitted[0].ToLowerInvariant();
			if (node.RouteElement == zero)
			{
				if (splitted.Length == 1)
				{
					return BuildRoute(node, dict);
				}
				result = CheckChildren(node, splitted, dict);
			}
			else if (node.RouteElement.StartsWith("{"))
			{
				var paramName = node.RouteElement.Trim(new[] { '{', '}' });

				if (paramName.StartsWith("*"))
				{
					paramName = paramName.Substring(1);
					dict.Add(paramName, pathWithParams);
					return BuildRoute(node, dict);
				}
				if (paramName.StartsWith("."))
				{
					paramName = paramName.Substring(1);
				}

				dict.Add(paramName, HttpUtility.UrlDecode(splitted[0]));
				if (splitted.Length == 1)
				{
					return BuildRoute(node, dict);
				}
				result = CheckChildren(node, splitted, dict);
			}
			return result;
		}

		private static Route BuildRoute(RouteTree node, Dictionary<string, string> dict)
		{
			return new Route(node.RouteItem, dict);
		}

		private Route CheckChildren(RouteTree node, IEnumerable<string> splitted, Dictionary<string, string> dict)
		{
			Route result = null;
			if (node.Child.Count > 0)
			{
				var subItem = string.Join("/", splitted.Skip(1).ToArray());
				for (int index = 0; index < node.Child.Count && result == null; index++)
				{
					var children = node.Child[index];
					result = FindRoute(children, subItem, dict);
				}
			}
			return result;
		}

		public List<ViewDescriptor> Views
		{
			get { return new List<ViewDescriptor>(_views.Values.ToArray()); }
		}

		public List<string> Controllers
		{
			get { return new List<string>(_controllers.Values.ToArray()); }
		}

		public void RegisterRoute(RouteDefinition definition)
		{
			var dynamicRoute = definition as DynamicRoute;

			if (dynamicRoute != null)
			{
				RegisterDynamicRoute(dynamicRoute);
				return;
			}
			var staticRoute = definition as StaticRoute;
			if (staticRoute != null)
			{
				RegisterStaticRoute(staticRoute);
				return;
			}
			var viewRegistration = definition as ViewRegistration;
			if (viewRegistration != null)
			{
				RegisterView(viewRegistration);
			}
		}

		private void RegisterView(ViewRegistration viewRegistration)
		{
			if (!string.IsNullOrWhiteSpace(viewRegistration.View) &&
							!_views.ContainsKey(viewRegistration.View))
			{
				_views.Add(viewRegistration.View, new ViewDescriptor
				{
					View = viewRegistration.View,
				});
			}
		}

		private void RegisterStaticRoute(StaticRoute staticRoute)
		{
			staticRoute.RoutePath = SanitizePath(staticRoute.RoutePath);
			staticRoute.Destination = staticRoute.Destination;
			_staticRoutes.Add(new KeyValuePair<string, StaticRoute>(staticRoute.RoutePath, staticRoute));
		}

		private void RegisterDynamicRoute(DynamicRoute dynamicRoute)
		{
			if (!string.IsNullOrWhiteSpace(dynamicRoute.View) && !_views.ContainsKey(dynamicRoute.View))
			{
				_views.Add(dynamicRoute.View, new ViewDescriptor
				{
					View = dynamicRoute.View,
				});
			}

			dynamicRoute.RoutePath = SanitizePath(dynamicRoute.RoutePath);
			if (!CheckRouteValidity(dynamicRoute.RoutePath))
			{
				//throw new InvalidRouteException(dynamicRoute.RoutePath);
				throw new Exception(); //TODO
			}

			_root.AddChild(dynamicRoute.RoutePath, dynamicRoute);
			if (string.IsNullOrWhiteSpace(dynamicRoute.Controller)) return;
			if (!_controllers.ContainsKey(dynamicRoute.Controller))
			{
				_controllers.Add(dynamicRoute.Controller, dynamicRoute.Controller);
			}

		}

		private bool CheckRouteValidity(string route)
		{
			// ReSharper disable StringIndexOfIsCultureSpecific.1
			var indexOfOptional = route.IndexOf("{.");
			var indexOfStar = route.IndexOf("{*");
			// ReSharper restore StringIndexOfIsCultureSpecific.1
			if (indexOfOptional < 0 && indexOfStar < 0) return true;
			if (indexOfOptional >= 0 && indexOfStar >= 0) return false;
			var indexOfStdParam = -1;
			var charray = route.ToCharArray();
			for (int i = 0; i < (charray.Length - 1); i++)
			{
				var ch = charray[i];
				var chNext = charray[i + 1];
				if (ch == '{' && chNext != '.' && chNext != '*')
				{
					indexOfStdParam = Math.Max(i, indexOfStdParam);
					break;
				}
			}
			if (indexOfStdParam > indexOfOptional && indexOfOptional >= 0)
			{
				if (indexOfStdParam >= 0) return false;
			}
			if (indexOfStdParam > indexOfStar && indexOfStar >= 0)
			{
				if (indexOfStdParam >= 0) return false;
			}
			return true;
		}
	}
}
