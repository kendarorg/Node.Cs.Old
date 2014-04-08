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
using System.Data.Entity;
using Node.Cs.Authorization;
using Node.Cs.EntityFramework;
using Node.Cs.Lib;
using Node.Cs.Lib.Controllers;
using Node.Cs.Lib.Filters;
using Node.Cs.Lib.Loggers;
using Node.Cs.Lib.Routing;
using Node.Cs.Project.Template.Src.Models;

namespace Node.Cs.Project.Template.Src
{
	public class GlobalNodeCs
	{
		public IEnumerable<ILogger> InitializeLoggers()
		{
			return new List<ILogger> { new FileLogger("Template.Src"), new ConsoleLogger() };
		}

		public void RegisterRoutes()
		{
			GlobalVars.RoutingService.AddStaticRoute("~/Content");
			GlobalVars.RoutingService.AddStaticRoute("~/Scripts");
			GlobalVars.RoutingService.AddRoute("~/{controller}/{action}/{id}",
				new
				{
					Controller = "Home",
					Action = "Index",
					Id = RoutingParameter.Optional
				});
		}

		public void InitializeSecurity()
		{
			GlobalVars.AuthenticationDataProvider = new EfAuthenticationDataProvider(() => new AccountDbContext());
			GlobalVars.GlobalFilters.Add(new GlobalAuthorize());
		}

		public void InitializeDatabase()
		{
			EfInitializer.InitializeConnectionString("AccountsDb");
			Database.SetInitializer(new CreateDatabaseIfNotExists<AccountDbContext>());
		}

		// ReSharper disable once InconsistentNaming
		public void Application_Start()
		{
			RegisterRoutes();
			InitializeDatabase();
			InitializeSecurity();
		}
	}
}
