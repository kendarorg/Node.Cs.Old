<!--settings(
title=Entity Framework Plugin
description=Node.Cs.EntityFramework, Entity Framework Plugin.
)-->

<!--include(shared/breadcrumb.php)-->

## {Title}

### Prerequisites

#### Node.Config

To use cshtml the Node.Cs.Razor plugin must be loaded into the solution and
added into the "Plugin" section of the "node.config" file, plus the connection strings and the 
DbProviderFactories.

Note that this configuration is exactly as the one needed in standar web.config files on MVC. And
the values specified are taken "AS IS" from the standard Asp.NET settings for entity framework.

<pre class="brush:html;">
<NodeCsConfiguration>
	<NodeCsSettings>
		<Plugins>
			<Plugin Dll="Node.Cs.EntityFramework.dll"/>
		</Plugins>
		
		<ConnectionStrings>
			<ConnectionString 
				DataSource="Data Source=(LocalDb)\v11.0;Initial Catalog=NodeCsMusicStore;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|NodeCsMusicStore.mdf" 
				Name="MusicStoreEntities" Provider="System.Data.SqlClient"/>
		</ConnectionStrings>
		<DbProviderFactories>
			<Factory InvariantName="System.Data.SqlClient" Type="System.Data.SqlClient.SqlClientFactory, System.Data" />
		</DbProviderFactories>
</pre>

Finally a new section must be added with the definition of the various providers.

<pre class="brush:html;">
	<EntityFrameworkSettings>
		<DefaultConnectionFactory Type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework">
		</DefaultConnectionFactory>
		<Providers>
			<Provider InvariantName="System.Data.SqlClient" Type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
		</Providers>
	</EntityFrameworkSettings>
</pre>

#### GlobalNodeCs

Inside the Application_start the Entity framework must be initialized using the Name of the connection
string inside the node.config:

<pre class="brush: csharp;">
EfInitializer.InitializeConnectionString("MusicStoreEntities");
</pre>

#### DbContext

The DbContext must be flagged with a specific attribute, and must be called passing the connection
string produced through the plugin, EfInitializer.ConnectionString

<pre class="brush: csharp;">
[DbConfigurationType(typeof(NodeCsDbConfiguration))]
public class MusicStoreEntities : DdContext
{
	public MusicStoreEntities()
		: base(EfInitializer.ConnectionString)
	{

	}
	public DbSet<Album> Albums { get; set; }
	public DbSet<Genre> Genres { get; set; }
	public DbSet<Artist> Artists { get; set; }
	public DbSet<Cart> Carts { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<OrderDetail> OrderDetails { get; set; }
}
</pre>

### Usage

After the configuration EntityFramework can be used without any further configuration!!

### Authorization

The EntityFramework plugin exposes various utility classes to have an implementation of the 
Authentication data provider.

To use it, the custom DbContext must inherit from EfSecurityDdContext.

<pre class="brush: csharp;">
[DbConfigurationType(typeof(NodeCsDbConfiguration))]
public class MusicStoreEntities : EfSecurityDdContext
{
</pre>

Then inside the Application_Start must be specified the EfAuthenticationDataProvider, that receive
in input the DbContext that will be used to store the authorization data.

<pre class="brush: csharp;">
	GlobalVars.AuthenticationDataProvider = new EfAuthenticationDataProvider(() => new MusicStoreEntities());
</pre>
