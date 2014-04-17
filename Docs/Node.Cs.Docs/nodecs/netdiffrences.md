<!--settings(
title=Node.Cs-MVC 4 differences
description=Node.Cs-MVC 4 differences.
)-->

<!--include(shared/breadcrumb.php)-->

## {Title}

All this project is based essentially in providing a system easily portable from .NET MVC to Node.Cs, but despite all my 
effort something was simply unavoidable due to the totally different architecture

### Configuration

The "web.config" is used for Node.Cs application only as a shortcat to enable the original cshtml syntax highlighter.
The new configuration file is the "node.config" that contains not only the settings that were previously inside the
web.config but even those actually hidden inside the IIS configuration.

### Running a website

No more IIS. All is based (at least concering the retrieval of data) on the .NET HttpListener class.
I had two possible approaches available:

* Create a single executable for every web site
* Setup a "core" application executable able to load all the data directly from the various dlls.

I took the latter. This because i did'nt liked to replicate too much copies of the main executable favoring for a single core 
that can be run with different configuration files. The other option would be to run the whole server as a Windows service.
As sone as someone is interested...and inform me, that he would like this feature.

### Global.asax

I wrote the equivalent of the Global.asax file to give an entry point for various customizations: Security providers, 
IOC containers, custom filters. The difference is that the global variables are store into the GlobalVars static class,
and the AppStart method is replaced by the Application Start method.

### Controllers

The controllers had to be totally replaced. Mostly because of the return type. Now the ActionResult is replaced by
IEnumerable&lt;IResponse&gt;. This is needed by design because of the usage of the yield return as a control system
for the program lifecycle. The construct "yield return" had replaced all "return".

The methods are more or less the same. But there is some less visible difference:

* HttpContext.CurrentContext does not exists anymore. If you need the context, it should be passed.
* Not all controllers variables are implemented. If you need them, just ask and dependency on your urgency they will be added (of course i'm speaking only about the variables present in the default .NET MVC ControllerBase
* The web api will be less clear. I had not founded a way to keep the IEnumerable style for the return variables of the web apis. You can have JsonResponse, XmlResponse or wetheaver you like but is not possible to directly return an object different from an IResponse
* The function View() would not work if you have a model inside the cshtml. It would be not difficult but...i have to admit...boring, to add a system to retrieve the type from string for complex and generic types.
* Calling an async function will be made through the InvokeTaskAndWait function.
* Controllers should inherit from ControllerBase
* Web api should inherit from ApiControllerBase

### Cshtml

Now the only available source for the model is the Model variable. I hated the mix of model and Model :)
The only real difference (apart from some missing helper function that i'll be glad to implement if you request them to me!)
is when calling the various helper:

Instead of 

<pre class="brush: csharp;">
	@Html.LabelFor(model=>model.MyField)
</pre>

should be used

<pre class="brush: csharp;">
	@Html.LabelFor(()=>model.MyField)
</pre>

As soon as i understand in depth how Microsoft did I'll allow both version!

### Authentication

I did'nt liked the way authentication/authorization providers worked. I want my data source to handle my users in general,
so in Node.Cs you are not tied to Windows Authorization to authenticate and this is reflected into a new way to initialize
the authentication/authorization providers inside the GlobalNodeCs.cs

### Missing features

I used the MvcMusicStore to implement the most useful (or at least the most poupular) functions  but if in need of something 
present into .NET MVC that you would like on Node.Cs, just ask, and i'll add it as soon as possible!