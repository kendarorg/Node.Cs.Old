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
using Node.Cs.Lib.Routing.RouteDefinitions;

namespace Node.Cs.Lib.Routing
{
	public class Route
	{
		public RouteDefinition Definition { get; set; }
		public IDictionary<string, string> Pars { get; private set; }
		public string View { get; private set; }

		public Route(string view)
		{
			View = view.ToLowerInvariant();
			Pars = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		}

		// ReSharper disable once ParameterTypeCanBeEnumerable.Local
		public Route(RouteDefinition routeItem, IDictionary<string, string> pars, string view = null)
		{
			Definition = routeItem;
			var def = routeItem as ViewRegistration;
			if (def != null && def.View != null) View = def.View.ToLowerInvariant();
			if (view != null) View = view.ToLowerInvariant();
			Pars = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			if (pars != null)
			{
				foreach (var par in pars)
				{
					Pars.Add(par.Key, par.Value);
				}
			}
		}
	}
}