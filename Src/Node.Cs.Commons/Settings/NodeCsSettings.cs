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


using System.Xml.Serialization;
using Node.Cs.Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Node.Cs.Lib.Settings
{
	public class NodeCsSettings : INodeCsSettingsRoot
	{
		public static NodeCsSettings Settings { get; set; }

		public static NodeCsSettings Defaults(string rootPath)
		{
			return new NodeCsSettings
			{
				Paths = new PathsDefinition
				{
					BinPaths = new List<string> { Path.Combine(rootPath, "App_Bin") },
					WebPaths = new List<PathProviderDefinition>
					           {
									new PathProviderDefinition
									{
										ClassName = "Node.Cs.Lib.PathProviders.FileSystemPathProvider",
										FileSystemPath = Path.Combine(rootPath, "App_Web"),
										ConnectionString = ""
									}
					           },

					DataDir = Path.Combine(rootPath, "App_Data")
				},
				Listener = new ListenerDefinition
				{
					Port = 8080,
					ServerNameOrIp = "*",
					ServerProtocol = "http",
					RootDir = "",
					SessionTimeout = 60 * 60,
					Cultures = new CulturesDefinition()
				},
				Security = new SecurityDefinition
				{
					AuthenticationType="basic",
					LoginPage = "",
					Realm="Node.Cs"
				},
				Threading = new ThreadingDefinition
				{
					ThreadNumber = 1,
					MaxExecutingRequest = 1000,
					MaxConcurrentConnections = 10000,
					MaxMemorySize = 1000 * 1000 * 1000 * 2
				},
				Factories = new FactoriesDefinition
							{
								ControllersFactory = "Node.Cs.Lib.Controllers.BasicControllersFactory",
							},

				Plugins = new List<PluginDefinition>(),
				Handlers = new List<HandlerDefinition>
				           {
					           new HandlerDefinition
										 {
											 Dll = "Node.Cs.Razor.dll",
											 Handler = "Node.Cs.Razor.RazorHandler",
											 Extensions = new List<string>{"cshtml"}
										 }
				           },
				Debugging = new DebuggingDefinition
				{
#if DEBUG
					Debug = true,
					DebugAssemblyLoading = true
#endif
				},
				ConnectionStrings = new List<ConnectionStringsDefinition>(),
				DbProviderFactories = new List<ProviderFactoryDefinition>()
			};

		}

		public ConnectionStringsDefinition GetConnectionString(string name)
		{
			for (int i = 0; i < ConnectionStrings.Count; i++)
			{
				var cs = ConnectionStrings[i];
				if (string.Compare(cs.Name, name, true) == 0)
				{
					return cs;
				}
			}
			return null;
		}
		

		[XmlArrayItem("ConnectionString", typeof(ConnectionStringsDefinition))]
		[XmlArray("ConnectionStrings")]
		public List<ConnectionStringsDefinition> ConnectionStrings { get; set; }
		public FactoriesDefinition Factories { get; set; }
		public DebuggingDefinition Debugging { get; set; }

		[XmlArrayItem("Factory", typeof(ProviderFactoryDefinition))]
		public List<ProviderFactoryDefinition> DbProviderFactories { get; set; }

		public NodeCsSettings()
		{
			Debugging = new DebuggingDefinition();
			Paths = new PathsDefinition();
			Listener = new ListenerDefinition();
			Threading = new ThreadingDefinition();
			Factories = new FactoriesDefinition();
			Handlers = new List<HandlerDefinition>();
			Security = new SecurityDefinition();
			Plugins = new List<PluginDefinition>();
		}

		public SecurityDefinition Security { get; set; }
		public string Application { get; set; }
		public ThreadingDefinition Threading { get; set; }
		public PathsDefinition Paths { get; set; }
		public ListenerDefinition Listener { get; set; }


		[XmlArrayItem("Handler", typeof(HandlerDefinition))]
		[XmlArray("Handlers")]
		public List<HandlerDefinition> Handlers { get; set; }


		[XmlArrayItem("Plugin", typeof(PluginDefinition))]
		[XmlArray("Plugins")]
		public List<PluginDefinition> Plugins { get; set; }

		public void ReRoot(string rootDir)
		{
			Paths.ReRoot(rootDir);
			foreach (var connectionString in ConnectionStrings)
			{
				connectionString.SetDataDir(Paths.DataDir);
			}
		}

		public string SettingsTag
		{
			get { return "Node.Cs"; }
		}
	}

	public class ProviderFactoryDefinition
	{
		[XmlAttribute("InvariantName")]
		public string InvariantName { get; set; }

		[XmlAttribute("Type")]
		public string ProviderFactoryType { get; set; }
	}

	public class ConnectionStringsDefinition
	{
		[XmlAttribute("DataSource")]
		public string DataSource { get; set; }

		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("Provider")]
		public string Provider { get; set; }

		internal void SetDataDir(string dataDir)
		{
			dataDir = dataDir.TrimEnd('\\');
			DataSource = DataSource.Replace("|DataDirectory|", dataDir + "\\");
		}
	}

	public class PluginDefinition
	{
		[XmlAttribute("Dll")]
		public string Dll { get; set; }

	}

	public class HandlerDefinition
	{
		public HandlerDefinition()
		{
			Extensions = new List<string>();
		}
		[XmlAttribute("Dll")]
		public string Dll { get; set; }
		[XmlAttribute("ClassName")]
		public string Handler { get; set; }


		[XmlArrayItem("Extension", typeof(string))]
		[XmlArray("Extensions")]
		public List<string> Extensions { get; set; }
	}

	public class FactoriesDefinition
	{
		public string ControllersFactory { get; set; }
	}


	public class SecurityDefinition
	{
		public string AuthenticationType { get; set; }
		public string Realm { get; set; }
		public string LoginPage { get; set; }
	}

	public class PathProviderDefinition
	{
		[XmlAttribute]
		public string ClassName { get; set; }
		[XmlAttribute]
		public string ConnectionString { get; set; }
		[XmlAttribute]
		public string FileSystemPath { get; set; }

		internal void ReRoot(string rootDir)
		{
			if (string.IsNullOrEmpty(FileSystemPath)) return;
			FileSystemPath = PathCleanser.ReRoot(FileSystemPath, rootDir);
		}
	}

	public class ThreadingDefinition
	{
		public int ThreadNumber { get; set; }
		public int MaxExecutingRequest { get; set; }
		public int MaxConcurrentConnections { get; set; }
		public int MaxMemorySize { get; set; }
	}

	public class ListenerDefinition
	{
		public int Port { get; set; }
		public String ServerNameOrIp { get; set; }
		public string ServerProtocol { get; set; }
		public string RootDir { get; set; }
		public int SessionTimeout { get; set; }
		public CulturesDefinition Cultures { get; set; }

		public string GetPrefix()
		{
			return string.Format("{0}://{1}:{2}/{3}", ServerProtocol, ServerNameOrIp, Port, RootDir);
		}
	}

	public class PathsDefinition
	{
		public PathsDefinition()
		{

			WebPaths = new List<PathProviderDefinition>();
			BinPaths = new List<string>();
		}

		[XmlArrayItem("PathProvider", typeof(PathProviderDefinition))]
		public List<PathProviderDefinition> WebPaths { get; set; }

		[XmlArrayItem("Path", typeof(string))]
		[XmlArray("BinPaths")]
		public List<string> BinPaths { get; set; }
		public string DataDir { get; set; }

		internal void ReRoot(string rootDir)
		{
			DataDir = PathCleanser.ReRoot(DataDir, rootDir);
			for (var i = 0; i < BinPaths.Count; i++)
			{
				BinPaths[i] = PathCleanser.ReRoot(BinPaths[i], rootDir);
			}
			foreach (var path in WebPaths)
			{
				path.ReRoot(rootDir);
			}
		}
	}
}
