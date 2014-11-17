<!--settings(
title=Generic Helpers
description=Generic Helpers. Useful for assembly management, command line crontab and resources.
)-->
## {Title}

This is a collection of utilities to help the development

Available as [Nuget package](http://www.nuget.org/packages/GenericHelpers/)

## Crontab

A simple crontab parser. This can be used in conjunction with a timer to verify if a certain operation have to be run.

### Usage with baseline

It defines a Crontab class that, starting at a certain date time, would be allowed to run every certain amount of milliseconds

<pre class="brush: csharp;">

void Initialize()
{
		//Starting time
		var startAt = DateTime.Parse("26/10/2009 8:47:39 AM",CultureInfo.GetCultureInfo("en-GB"));

		//From now, every 5000 ms
		var target = new Crontab(startAt, 5000);

		//Calculate the first time that the action must be run
		var next = target.Next(DateTime.Now);
		var toWait = (next- DateTime.Now).TotalMilliseconds;

		//Initialize a timer to run after the needed time
		if(toWait>0){
			var timer = new Timer(toWait);
			timer.Elapsed += OnTimerElapsed;
		}
		
}
</pre>


### Usage with crontab syntax

It defines a Crontab class that would give the "next" time given a certain crontab definition

<pre>
 Format string with spaces between the entries
  *     *     *    *     *     *    * command to be executed
  -     -     -    -     -     -    -
  |     |     |    |     |     |    +----- year
  |     |     |    |     |     +----- day of week (0 - 6) (Sunday=0)
  |     |     |    |     +------- month (1 - 12)
  |     |     |    +--------- day of month (1 - 31)
  |     |     +----------- hour (0 - 23)
  |     +------------- min (0 - 59)
  +------------- sec (0 - 59)
</pre>

For an usage example:

<pre class="brush: csharp;">
void Initialize()
{
		var target = new Crontab("10 */1 * * * * *", true);

		//Calculate the first time that the action must be run
		var next = target.Next(DateTime.Now);
		var toWait = (next- DateTime.Now).TotalMilliseconds;

		//Initialize a timer to run after the needed time
		if(toWait>0){
			var timer = new Timer(toWait);
			timer.Elapsed += OnTimerElapsed;
		}
		
}
</pre>

## ResourceContentLoader

An utility class to load easily the resources as binary or as text.

The most important thing is that given ONLY the partial file name the resource is searched inside the assemblies.

That means that if is present a resource named "ms.apackage.anotherpackage.resource.txt" it can be found with the names:

* ms.apackage.anotherpackage.resource.txt
* apackage.anotherpackage.resource.txt
* anotherpackage.resource.txt
* resource.txt

The available functions are:

* string LoadText(string resourceName, Assembly assembly = null, bool throwIfNotFound = true): Get the resource as string
** resourceName: The name of the resource, with optionally part or the full path
** assembly the assembly in wich the resource should be searched, if null the current assembly will be used
** throwIfNotFound: Throw a FileNotFound exception if the resource does not exists. If set to false returns null if the resource does not exists.
* byte[] LoadBytes(string resourceName, Assembly assembly = null, bool throwIfNotFound = true): Get the resource as byte array
** resourceName: The name of the resource, with optionally part or the full path
** assembly the assembly in wich the resource should be searched, if null the current assembly will be used
** throwIfNotFound: Throw a FileNotFound exception if the resource does not exists. If set to false returns null if the resource does not exists.
* string GetResourceName(string resourcePath, Assembly assembly = null, bool throwIfNotFound = true): Get the full resource name
** resourceName: The name of the resource, with optionally part or the full path
** assembly the assembly in wich the resource should be searched, if null the current assembly will be used
** throwIfNotFound: Throw a FileNotFound exception if the resource does not exists. If set to false returns null if the resource does not exists.

## CommandLineParser

Needed to parse the command line parameters. 

Can show an help is something goes wrong. With the default -h or -help flags. Usually I store it in a resource file and load it through the ResourceContentLoader.

Loads even the environment variables in the same dictionary of the command line arguments

* Constructor(string[] args,string helpString)
** args: The arguments passed to the main method
** helpString: The string to show when -h or -help parameters are passed

For an usage example, calling the executable as:

  MyExecutable -test -parameter parameterValue

<pre class="brush: csharp;">
		public void Main(string[] args)
		{
			const string helpString = "The help message";
			var commandLineParser = new CommandLineParser(args, helpString);
			if(commandLineParser.Has("test")){
				//Do something
			}else if(commandLineParser.Has("parameter"){
				var parameterValue = commandLineParser["parameter"];
				//Do something
      }else{
				//Something went wrong..so show the help
				commandLineParser.ShowHelp();
		  }
    } 
</pre>

The available functions are:
* [string]: To get and set variables
* string GetEnv(string name): To get an environment variable
* void SetEnv(string name,string value): To set an environment variable. Stored on the CommandLineParser instance ONLY.
* bool IsSet(string name): To check if a variable exists
* bool Has(params string[] names): To check if all the passed variables are set
* bool HasAllOrNone(params string[] names): To check if all variables are set, or none of them
* bool HasOneAndOnlyOne(params string[] names): To check if ONLY one variable between the required is set
* void ShowHelp(): Show the help string
* string Help: Get the help string

## AssembliesManager

An utility to work with assemblies and type:

* Type LoadType(string fullQualifiedName): Seek for a type on all loaded assemblies
* Type LoadType(Assembly sourceAssembly, string fullQualifiedName): Seek for a type on a specific assembly
* static IEnumerable&lt;Type&gt; LoadTypesWithAttribute(params Type[] types): Load all the types with the given attributes on all assemblies
* static IEnumerable&lt;Type&gt; LoadTypesWithAttribute(Assembly sourceAssembly, params Type[] types): Load all the types with the given attributes on the specified assembly
* static IEnumerable&lt;Type&gt; LoadTypesInheritingFrom(params Type[] types): Load types inheriting/implementing the given types from all assemblies
* static IEnumerable&lt;Type&gt; LoadTypesInheritingFrom(Assembly sourceAssembly, params Type[] types): Load types inheriting/implementing the given types from the specified assembly
* void DumpAssembly(string path, Assembly assm, Dictionary<string, Assembly> alreadyLoaded): Dump an assembly on the given path with all its dependencies
* string GetAssemblyName(string fullName): Retrieve the "dll" name from an Assembly.GetName()
* bool LoadAssemblyFrom(string dllFile, List<string> missingDll, params string[] paths): Load an assembly searching inside the path, giving the list of missing dlls.
* IEnumerable&lt;Assembly&gt; LoadAssembliesFrom(string path, bool deep = true): Load all the assemblies in a given path, when deep search even in subdirectories


## Licensing

Copyright (C) 2013-2014 Kendar.org


Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Download

See [kendar.org](http://www.kendar.org/ "Kendar.org") for the latest changes.
