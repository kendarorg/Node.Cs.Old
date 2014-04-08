<!--settings(
title=Node.Cs Mvc Controllers Usage
description=Node.Cs Mvc Controllers Usage.
)-->

## {Title}

The MVC controllers in Node.Cs are exactly like the standard .Net Mvc contorllers.

They should respect the naming convention _ControllerName_Controller and must implement the IController interface.

To ease the development a ControllerBase class exists, that contains several utilities for this purpose.

We assume here that the handler for Razor is loaded (Node.Cs.Razor).

__TO UNDERSTAND HOW PARAMETERS ARE PASSED WITHIN ROUTING, PLEASE CHECK THE ROUTING SECTION!__

### Simple controller

The following is a controller that will respond to a GET request like http://server/Simple/Index 
and returns an Index.cshtml page as result passing the model SimpleModel to the view

<pre class="brush: csharp;">
public class SimpleController : ControllerBase
{
		public IEnumerable<IResponse> Index()
		{
			var model = new SimpleModel{Data = "data"};
			yield return View(model);
		}
}
</pre>

This will call the view  ~/Views/SimpleController/Index.cshtml, passing the model as parameter

__UNLIKE THE STANDARD MVC Is not possible to call a view that use a model without passing AT LEAST an empty model__

The corresponding cshtml could be like this:

<pre class="brush: php; html-script: true">
<html>
	<body>
		The model data is @Model.Data
	</body>
</html>
</pre>

### Controller variables

* ViewBag: The dynamic object used to pass everything to the views (as per .NET MVC)
* Session: The session, containing for example the User identity etc  (as per .NET MVC)

### Specifying verbs

Different verbs can be specified for the various method, being GET the default one.
Note that only one method can exists with a given verb with the same name: two methods accessed
through GET can't have the same name.

The available methods are the default ones:

* HttpGet
* HttpPost
* HttpPut
* HttpDelete

<pre class="brush: csharp;">
public class SimpleController : ControllerBase
{
		[HttpGet]
		public IEnumerable<IResponse> Index()
		{
			...
		}

		[HttpPost]
		public IEnumerable<IResponse> DoSomething()
		{
			...
		}
}
</pre>

### Passing parameters with GET/DELETE

Passing parameters as query string will work as with MVC. 
Automatic conversion is made for the various types and default values are honoured

E.g. calling http://myhost/SimpleController/WithParams?par1=text&par2=100

Could invoke the following action.

<pre class="brush: csharp;">
public class SimpleController : ControllerBase
{
		[HttpGet]
		public IEnumerable<IResponse> WithParams(string par1,int par2)
		{
			...
</pre>

We could even call  http://myhost/SimpleController/WithParams?par1=text&par2=100 without the par2 parameter
It would be needed to mark it as nullable or with a default value on the action

<pre class="brush: csharp;">
public class SimpleController : ControllerBase
{
		[HttpGet]
		public IEnumerable<IResponse> WithParams(string par1,int? par2)
		{
			...
</pre>

### Passing parameters with POST/PUT

The same will happen with the form parameters passed inside a PUT or a POST through a form with

* application/x-www-form-urlencoded
* multipart/form-data

The support to pass JSON or XML data will be added as soon as possible!
<!-- TODO -->

### Uploading files

Posting a form with a file with a content type of multipart/form-data the file handling works slightly differently.
Note that form data can be passed at the same time. 

An example of controller handling the thing would be:

<pre class="brush: csharp;">
public class SimpleController : ControllerBase
{
	[HttpPost]
	public IEnumerable<IResponse> HandleFiles()
	{
		var requiredFile = Context.Request.Files["myFileId"];
		
		var fileModel = new MyFileModel();
		model.FileName = file.FileName;
		model.FileSize = file.ContentLength;
		...
</pre>

### Session

Session variables can be set, removed and used. 

The object stored in session are stored in memory. A recycle will delete all session data.

The object stored in session would have to be serializable (else it would not be possible to
use other than the InMemorySessionStorage).

<pre class="brush: csharp;">
	Context.Session["testIndex"]=new MyHopefullySerializableObject();
	Context.Session.Remove("oldValue");
	var value = Context.Session["anotherValue"];
	Context.Session.Clear();
</pre>

### Cookies

Cookies variables can be set, removed and used.
They will resist until they expire or the server recycle.

The are only string kye value pairs.

<pre class="brush: csharp;">
	Context.Response.AppendCookie(new HttpCookie("test","testValue")
	{
		Expires = DateTime.Now+TimeSpan.FromDays(1)
	});
	var myCookieValue = Context.Response.Cookies("test");
</pre>

### Returning Views

Exactly as with the standard .NET MVC it is possible to return views from the controllers

* IResponse View(object model): Return the /Views/ThisController/ThisAction view, passing the model as parameter
* IResponse View(string view, object model): Return exactly the view passed (an address like ~/mycshtml.cshtml), passing the model as parameter
* IResponse PartialView(object model): Return the /Views/ThisController/ThisAction view, passing the model as parameter, as a partial view.
* IResponse PartialView(string action, object model): Return the /Views/ThisController/action view, passing the model as parameter, as a partial view.
* IResponse RedirectToAction(string action, string controller = null): Redirect to the associated view invoking the controller associated
* IResponse RedirectToAction(string action, dynamic value, string controller = null): Redirect to the associated view invoking the controller associated, resolvine the correct url using the properties of the dynamic value.

### Returning Json, Xml, Texts or bytes

There are too several utility methods into the Controller base:

* IResponse JsonResponse(object obj, string contentType = "application/javascript", Encoding encoding = Encoding.UTF8)
* IResponse XmlResponse(object obj, string contentType = "application/xml", Encoding encoding = Encoding.UTF8)
* IResponse StringResponse(string data, string contentType = "text/plain", Encoding encoding = Encoding.UTF8)
* IResponse ByteResponse(byte[] data, string contentType = "application/octet-stream", Encoding encoding = Encoding.UTF8)

### Redirect 

It would be possible to call for redirects as in MVC. This will return an IResponse, they will conclude the execution of the controller

* IResponse Redirect(string url): Redirect exactly to the required url
* IResponse RedirectToAction(string action, string controller): Redirect to the given controller at the given action
* IResponse RedirectToAction(string action, dynamic value, string controller): As before but using the properties of the dynamic object passed to find the exact route (for example to set the id of an item).

### Calling functions asincronously

This means that the current controller will be put in a stopped state, not consuming processor power until the moment in which
the called function will return. Note that the syntax is very similar to the one in my library of [Concurrency Helpers](http://www.kendar.org/?p=/dotnet/helpers/concurrencyhelpers),
they are, in fact the base of the whole thing.

All these function must "yield return" directly out of the controller. Only when an explicit response is produced the Container terminates.

#### Calling default .NET async functions

The various functions supporting async and await returns Task-s these task are mostly calling function already controlled through the 
I/O completion ports, meaning that the simply wait for the data returned by the drivers, they mostly do nothing by themselves.

For example to write data on a stream, that essentially copies data on the network card buffer and wait for the response.
In this situation the controller will be stopped until the end of the WriteAsync function.

<pre class="brush: csharp;">
	yield return InvokeTaskAndWait(tcpStream.WriteAsync(bytes, 0, bytes.Length 0));
</pre>

The signature is 

<pre class="brush: csharp;">
	IResponse InvokeTaskAndWait(Task task);
</pre>

#### With return values

In the following code we are calling the ReadText function, its definition is the following. 
When it finish it will return a string with the content of the file. This function will call a ReadAsync on a file stream
thus will return null, until the moment in which it would have read all the file content.

<pre class="brush: csharp;">
	IEnumerable<string> ReadText(string path);
</pre>

The result, when the ReadText would be completed, will be stored inside the Container.RawData field.
No wait will be made, only a poll to check if the asynchronous operation finished

<pre class="brush: csharp;">
	var result = new Container();
	yield return InvokeLocalAndWait(() => _globalPathProvider.ReadText(path), container);
	var data = container.RawData as string;
</pre>

The signature is 

<pre class="brush: csharp;">
	IResponse InvokeLocalAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
</pre>

#### As tasks

It is possible to call wetheaver Action as a task. For example when an operation would be very complex and
need to be mantained all concentrated in a single "context" and would block the thread for too much time,
we can invoke an action as a Task, and block the execution of the caller until the completion of the task.

<pre class="brush: csharp;">
	yield return InvokeAsTaskAndWait(() => context.Initialize())
</pre>

The signature is 

<pre class="brush: csharp;">
	IResponse InvokeAsTaskAndWait(Action action)
</pre>

### Working with files

An example working with file uploads. The following example some files are uploaded
and the first of them is taken and its data shown on the model.

Note that the full content of the files is available.

<pre class="brush: csharp;">
[HttpPost]
public IEnumerable<IResponse> PostFiles()
{
	//Retrieve all the files ids
	var allFiles = HttpContext.Request.Files.AllKeys.ToArray();

	var model = new FileModel();
	//If some file exists
	if (allFiles.Length > 0)
	{
		//Take the first one
		var file = HttpContext.Request.Files[allFiles[0]];
		if (file != null)
		{
			//And load its data
			model.FileName = file.FileName;
			model.FileSize = file.ContentLength;
		}
	}
	//return the model
	yield return View(model);
}
</pre>