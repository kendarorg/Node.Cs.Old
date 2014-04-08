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


using Node.Cs.EntityFramework.Settings;
using Node.Cs.Lib;
using Node.Cs.Lib.Settings;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Node.Cs.EntityFramework
{
	public class NodeCsDbConfiguration : DbConfiguration
	{
		public NodeCsDbConfiguration()
		{
			var csDefintion = GlobalVars.Settings.GetConnectionString(EfInitializer.ConnectionName);
			var name = csDefintion.Provider;
			var providerServiceFullName = EfInitializer.Settings.Providers.First((a) => string.Compare(name, a.InvariantName, true) == 0).ProviderType;
			var providerServiceType = Type.GetType(providerServiceFullName);

			var constructor = providerServiceType.GetConstructor(new Type[] { });
			var instance = providerServiceType.GetField("Instance");
			if (constructor != null)
			{
				var ps = (DbProviderServices)constructor.Invoke(new object[] { });
				SetProviderServices(name, ps);
			}
			else if(instance!=null)
			{

				var ps = (DbProviderServices)instance.GetValue(null);
				SetProviderServices(name, ps);
			}
			
		}
	}

	public class EfInitializer
	{
		public static EntityFrameworkSettings Settings { get; private set; }
		public static string ConnectionString { get; private set; }
		public static string ConnectionName { get; private set; }

		public static void InitializeConnectionString(string connectionName)
		{
			ConnectionName = connectionName;
			Settings = NodeCsConfiguration.GetSection<EntityFrameworkSettings>("EntityFrameworkSettings");
			var csDefintion = GlobalVars.Settings.GetConnectionString(connectionName);
			//var  = csDefintion.Name;
			var dataSource = csDefintion.DataSource;
			var name = csDefintion.Provider;

			var providerFactoryFullName = GlobalVars.Settings.DbProviderFactories.First((a) => string.Compare(name, a.InvariantName, true) == 0).ProviderFactoryType;
			var providerServiceFullName = Settings.Providers.First((a) => string.Compare(name, a.InvariantName, true) == 0).ProviderType;
			var connectionFactoryFullName = Settings.DefaultConnectionFactory.FactoryType;
			var parameters = Settings.DefaultConnectionFactory.Parameters.Select((a) => a.Value).ToList();
			Initialize(providerFactoryFullName, providerServiceFullName, connectionFactoryFullName, parameters);

			EntityConnectionStringBuilder entityBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder();
			entityBuilder.Provider = name;

			// Set the provider-specific connection string.
			entityBuilder.ProviderConnectionString = dataSource;

			ConnectionString = dataSource;// entityBuilder.ConnectionString;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="providerFactoryFullName">System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91</param>
		/// <param name="providerServiceFullName"> System.Data.Entity.SqlServerCompact.SqlCeProviderServices, EntityFramework.SqlServerCompact</param>
		/// <param name="connectionFactoryFullName">System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework</param>
		/// <param name="parameters"></param>
		private static void Initialize(string providerFactoryFullName, string providerServiceFullName, string connectionFactoryFullName, List<string> parameters)
		{
			var providerFactoryType = Type.GetType(providerFactoryFullName);
			var providerServiceType = Type.GetType(providerServiceFullName);
			var connectionFactoryType = Type.GetType(connectionFactoryFullName);

			DbConfiguration.Loaded += (_, a) =>
			{
				//a.ReplaceService<DbProviderFactory>((s, k) => (DbProviderFactory)Activator.CreateInstance(providerFactoryType));
				//a.ReplaceService<DbProviderServices>((s, k) => (DbProviderServices)Activator.CreateInstance(providerServiceType));

				//var prs  = parameters.Select((b)=>(object)b).ToArray();
				var types = new List<Type>();
				foreach (var par in parameters)
				{
					types.Add(par.GetType());
				}
				var constructor = connectionFactoryType.GetConstructor(types.ToArray());

				a.ReplaceService<IDbConnectionFactory>((s, k) => (IDbConnectionFactory)constructor.Invoke(parameters.ToArray()));
			};
		}
	}
}
