<!--settings(
title=Node.Cs Installation
description=Node.Cs Installation.
)-->

## {Title}

First and most important: If you use these libraries, please give me some feedback to improve them with the
disqus form at the end of the page or with this site contact form. Without your support i'll follow only my
selfish development needs!

Note that when installing the various packages from nuget command prompt you should specify the -Pre option,
since everything is in alpha at this time. At the same time, for the very same reason, when installing from 
the visual interface, you should allow to show the prereleases.

### Prerequisites

* .Net Framework 4.5 or more
* Nuget Package Manager installed on Visual Studio
* Visual Studio 2012 or more, all version (Express/Pro/Wetheaver)
* The Node.Cs Project Template from downloadble [here]({This}Node.Cs.Extension.zip)

A Mono port is on the way!

### Available packages

Everything available for nuget (now in pre-release, when the documention will be more or less completed i'll publish them as "official")

* [Node.Cs.Cmd](http://www.nuget.org/packages/Node.Cs.Cmd/): The command line runner for the Node.Cs server
* [Node.Cs.Commons](http://www.nuget.org/packages/Node.Cs.Commons/): The minimum interfaces and classes to develop plugins and web sites
* [Node.Cs.Lib](http://www.nuget.org/packages/Node.Cs.Lib/): The core functionalities
* [Node.Cs.Razor](http://www.nuget.org/packages/Node.Cs.Razor/): The razor engine, compatible with the Ms one.
* [Node.Cs.EntityFramework](http://www.nuget.org/packages/Node.Cs.EntityFramework/): EntityFramework plugin, contains an authorization provider.
* [Node.Cs.Admin](http://www.nuget.org/packages/Node.Cs.Admin/): Administration interfaces and performances.

### Node.Cs Project Template

#### Structure

The basic project is a standard .Net class library, based on framework 4.5 or greater.

It's pretty simple, can be created directly with the specific Node.Cs template [here]({This}Node.Cs.Extension.vsix)
It follows more or less the MVC file structure (to be compatible with the MVC 4 nuget packages)

* App_Bin: The application binaries, configurable
* App_Data: The application data (mdf/ldf/...) directory

* Views: The cshtml files as in standard Ms MVC projects
* Content: Bare content files like css and jpg
* Scripts: All javascripts

* node.config: The configuration file
* GlobalNodeCs.cs: The global.asax equivalent for node cs.

* Controllers: The package for the controllers
* Models: containing the view models

#### The compilation and debugging process

After the "standard" class library compilation,

* The Node.Cs.Cmd.exe, that is the true server executable is copied inside the App_Bin directory.
* All dlls inside the project target folder (e.g. MyProject/bin/Release) are copied inside the App_Bin.

To debug, the site is run calling the Node.Cs.Cmd.exe:

	MyProject/App_Bin/Node.Cs.Cmd.exe -config MyProject/node.config

This will be the default run configuration.

During the debugging only the dlls specified inside the configuration will be loaded despite what is set inside the project references.

#### Syntax highlight for cshtml/razor

To allow the usage of syntax highlight inside Razor, inside the project template is present a web.config file, and exists all ther references 
to the .Net standard razor dlls. This is used ONLY to cheat the Visual Stuido parser and keep smooth the development of the various chstml 
files "the standard way".

#### Files needed for the deployment

* The App_Bin diretory
* The App_Data directory if is used the default EntityFramewir
* Content,Views and Scripts directories
* node.config

The idea is that into the AfterBuild step all the binaries will be copied into the App_Bin directory. With the Node.Cs template all these task are accomplied inside the .csproj file directly. 

#### The content furhter explained

If you want to follow the easy way just install the Node.Cs.Extension from the extensions library, 
create a new Node.Cs.Project, enable the Nuget Packages Restore and rebuild all.

Please note that the default project template will include the Razor module and is able to handle
out of the box, the standard handling of controllers and views like with .Net MVC.

The content of the freshly created project will be:

* App_Web: The directory containing the files visible from the web server
* Views: The MVC views
	* Home: The views relative to the HomeController
		* index.cshtml: A simple view to check that the whole system works correctly
* App_Data: The directory containing the .mdf/.ldf database files
* App_Bin: The directory containing the dlls needed to run the application and the web server
* node.config: The configuration file for the application, already configured for the actual configuration.
* web.config: A stripped down version of the standard IIS web config. This is needed only to allow the syntax higlight for the cshtml views
* GlobalNodeCs.cs: The equivalent for node.cs of the Global.asax in IIS
* Controllers: The controllers directory
	* HomeController.cs: The "Home" controller, containing the action handler for the previously described index.cshtml

## Licensing

Copyright (C) 2013-2014 Kendar.org


Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Download

See [kendar.org](http://www.kendar.org/dotnet/nodecs "Kendar.org") for the latest changes.
