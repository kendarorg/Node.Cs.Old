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


using System.Collections.Generic;
using ConcurrencyHelpers.Utils;
using Node.Cs.Authorization;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.PathProviders;
using Node.Cs.Lib.Routing;
using Node.Cs.Lib.Settings;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib
{
	public static class GlobalVars
	{
		private static CounterInt64 _openedConnections = new CounterInt64();

		static GlobalVars()
		{
			GlobalFilters = new List<FilterBase>();
		}

		public static List<FilterBase> GlobalFilters { get; set; }
		public static IAuthenticationDataProvider AuthenticationDataProvider { get; set; }
		public static IRoutingService RoutingService { get; set; }
		public static NodeCsSettings Settings { get; set; }
		public static IConversionService ConversionService { get; set; }
		public static ISessionStorage SessionStorage { get; set; }
		public static IGlobalExceptionManager ExceptionManager { get; set; }
		public static string ApplicationLocation { get; set; }
		public static IControllersFactory ControllersFactory { get; set; }
		public static IGlobalPathProvider PathProvider { get; set; }
		public static IResponseHandlersFactory ResponseHandlers { get; set; }
		public static IExtensionHandler ExtensionHandler { get; set; }
		public static IControllersFactoryHandler ControllersFactoryHandler { get; set; }
		public static INodeCsServer NodeCsServer { get; set; }

		public static long OpenedConnections
		{
			get { return _openedConnections.Value; }
			set { _openedConnections.Value = value; }
		}
	}
}
