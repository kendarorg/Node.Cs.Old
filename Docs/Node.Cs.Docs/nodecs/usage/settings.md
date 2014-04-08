<!--settings(
title=node.config
description=Configuration and node.config
)-->

## {Title}

Here the explanation of -every- part of the node.config.. excluded the security section 
described on the page dedicated to the security.

### Application

This is the name of the application dll. It will be used to initialize the whole server process.

<pre class="brush:html;">
	<Application>NodeCsMusicStore</Application>
</pre>

### Factories

These are the factory that can be overridden to implement custom behaviours. Actually only
the ControllersFactory exist. It is used to create the controllers, and could be a custom
implementation based, for example, on Castle Windsor

<pre class="brush:html;">
	<Factories>
		<ControllersFactory>Node.Cs.Lib.Controllers.BasicControllersFactory</ControllersFactory>
	</Factories>
</pre>

### Threading

This section defines the specifications of the runtime environment.

* ThreadNumber: This is the number of threads that will execute the pages elaborations.
* MaxExecutingRequest: The maximum number of requests distributed between the threads. The requests exceeding this value will be queued and executed as soon as some place is free.
* MaxConcurrentConnections: The maximum number of concurrent connections. Over this value the connections will be dropped.
* MaxMemorySize: The maximum memory size allowed for this application Node.Cs process. When this limit is reached the application will Recycle, resetting all data, sessions etc.

<pre class="brush:html;">
<Threading>
	<ThreadNumber>2</ThreadNumber>
	<MaxExecutingRequest>10000</MaxExecutingRequest>
	<MaxConcurrentConnections>1000</MaxConcurrentConnections>
	<MaxMemorySize>200000000</MaxMemorySize>
</Threading>
</pre>

### Debugging

Defines the level of debugging

* Debug: Show all messages on the console, and save special data to ease the problem solving
* DebugAssemblyLoading: Intercept problems while loading assemblies. Note that you will see several warning about XmlSerializers end resources not found. Don't care, this is the normal behaviour and __IS NOT AN ERROR__

<pre class="brush:html;">
<Debugging>
	<Debug>true</Debug>
	<DebugAssemblyLoading>true</DebugAssemblyLoading>
</Debugging>
</pre>

### Listener

This configuration will define the configuration of the http listener

* Port: The listening TCP port (default for web is 80 :)  )
* ServerNameOrIp: A response will be issued on requests addressed at these ips or names. * means everything.
* ServerProtocol: Actually only the value 'http' is allowed. 
* SessionTimeout: The duration in seconds of a session, after the last access to the session itself.
* Cultures: The cultures allowed from clients. If not specified "en-US" will be used. These are used while rendering strings, dates, numbers etc. On the output streams.
	* Default: The default culture
	* Available: A comma separated list of accepted cultures

<pre class="brush:html;">
<Listener>
	<Port>8081</Port>
	<ServerNameOrIp>*</ServerNameOrIp>
	<ServerProtocol>http</ServerProtocol>
	<SessionTimeout>1200</SessionTimeout>
	<RootDir />
	<Cultures Default="en-US" Available="fr-FR,it-IT"/>
</Listener>
</pre>

### Plugins 

The list of dll that will be used as plugins. They can be in any order. They will be loaded at startup and
will execute their initializations.

<pre class="brush:html;">
	<Plugins>
		<Plugin Dll="Node.Cs.Admin.dll"/>
		<Plugin Dll="Node.Cs.EntityFramework.dll"/>
		<Plugin Dll="Node.Cs.Razor.dll"/>
	</Plugins>
</pre>

### Handlers

This section defines how the various file types will be handled.
For example here i will use the class RazorHandler to handle cshtml files, sayng
that the RazorHandler class is inside the Node.Cs.Razor.dll.

Another embedded handlers is the Node.Cs.Lib.Static.StaticHandler. That is used (normally) to handle
static files.

<pre class="brush:html;">
<Handlers>
	<Handler Dll="Node.Cs.Razor.dll" ClassName="Node.Cs.Razor.RazorHandler">
		<Extensions>
			<Extension>cshtml</Extension>
		</Extensions>
	</Handler>
</Handlers>
</pre>

### Database 

These follows exactly the standard web.config database configuration with the exact same 
values. Please note that the Provider specified into the connection string is exactly
the name specified inside the DbProviderFactory in the InvariantName attribute.

<pre class="brush:html;">
<ConnectionStrings>
	<ConnectionString 
		DataSource="Data Source=...;Initial Catalog=...;Integrated Security=SSPI;AttachDBFilename=..." 
		Name="MusicStoreEntities" 
		Provider="System.Data.SqlClient"/>
</ConnectionStrings>
<DbProviderFactories>
	<Factory 
		InvariantName="System.Data.SqlClient" 
		Type="System.Data.SqlClient.SqlClientFactory, System.Data" />
</DbProviderFactories>
</pre>

### Paths

__WebPaths__: The web paths are the paths in which is present a root of the file system. The connection string 
is specific for the various Path provider. If i specify multiple path provider, to find a file to render
on the front end, the file will be searched from the last to the first path provider.

The FileSystemPathProvider takes as connection string a relative directory (relative to the node.config location) 
or an absolute location. Other path providers, for example an hypothetical SqlPathProvider, could require the name
of a connection string specified on the database section on the node config.

__BinPaths__: The paths where all dlls are located, as before they are scanned from last to first to find the 
missing dlls.

__DataDir__: The equivalent of the App_Data directory for Asp.NET.

<pre class="brush:html;">
<Paths>
	<WebPaths>
		<PathProvider 
			ClassName="Node.Cs.Lib.PathProviders.FileSystemPathProvider" 
			ConnectionString="">
		</PathProvider>
	</WebPaths>
	<BinPaths>
		<Path>App_Bin</Path>
	</BinPaths>
	<DataDir>App_Data</DataDir>
</Paths>
</pre>