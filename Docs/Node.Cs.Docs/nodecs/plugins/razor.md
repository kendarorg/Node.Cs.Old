<!--settings(
title=Razor Pages and Helpers
description=Razor Pages and Helpers. Node.Cs.Razor Plugin.
)-->

## {Title}

### Prerequisites

To use cshtml the Node.Cs.Razor plugin must be loaded into the solution and
added into the "Plugin" section of the "node.config" file

<pre class="brush:html;">
<NodeCsConfiguration>
	<NodeCsSettings>
		<Plugins>
			<Plugin Dll="Node.Cs.Razor.dll"/>
			...
</pre>

The cshtml pages will be more or less the same for Node.Cs MVC

### Features

The features that are present "out of the box" are:

* Inclusion of the _ViewStart.cshtml pages
* Loading of the Layout page if specified
* Partial views
* Inclusion of views
* Most of UrlHepler and HtmlHelper methods implemented (they will be added under YOUR request)

### Html.ActionLink

Note that the routeValues, the controller name and the htmlAttributes are -all- nullable

* ActionLink(string title, string action, string controller): Create an href to the given action/controller
* ActionLink(string title, string action): Create an href on the given action with the current controller
* ActionLink(string title, string action, string controller, dynamic routeValues, object htmlAttributes): Create an href with the given action and controller, identifying the correct route through the route values passed with an anonymous type and adding the html attributes as specified in another anonymous type.
* ActionLink(string title, string action, string controller, dynamic routeValues): As the previous without html attributes.
* ActionLink(string title, string action, dynamic routeValues): As the previous but creating the link with the current controller.

### Html.BeginForm, Html.EditorForModel

The form creation is exactly like the standard MVC:

<pre class="brush: csharp;">
@using (Html.BeginForm()) {
	...
}
</pre>

* BeginForm(): Create a form with a POST verb and application/x-www-form-urlencoded type the target will be the same controller and action
* BeginForm(string action): As the previous but with the specified action
* BeginForm(string action, string controller, string verb, string encType): As the previous but specifying all parameters of the form
* EditorForModel(): Create an editor for the model specified inside the page. Works with the attributes as in standard MVC.
* ValidationSummary(bool value, string message = null): If there is some error show the message and the errors registered on the Model State without specifyng the field id.

### Fields (Html.EditorFor, Html.LabelFor etc)

* DisplayFor(()=>model.Property): Show the label and value for the given property. Works with the attributes as in standard MVC.
* EditorFor(()=>model.Property): Show the label and input for the given property. Works with the attributes as in standard MVC.
* ValidationMessageFor(()=>model.Property): Show the error messages for the given property. Works with the attributes as in standard MVC.
* TextBoxFor(()=>model.Property,string value=null): Create a textbox for the given property (with its value)
* TextBoxFor(string id,string value): Create a textbox with the given id and value
* HiddenFor(()=>model.Property,string value=null): Create a hidden field for the given property (with its value)
* HiddenFor(string id,string value): Create a hidden field with the given id and value
* PasswordFor(()=>model.Property,string value=null): Create a password field for the given property (with its value)
* PasswordFor(string id,string value): Create a password field with the given id and value
* FileFor(()=>model.Property): Create a file input field for the given property
* FileFor(string id): Create a file input field with the given id
* LabelFor(()=>model.Property, string label = null): Assign a label for the given field, if no label is specified the DisplayAttribute is specified
* LabelFor(string label): Create a label with the specified label
* CheckBoxFor(()=>model.Property): Create a checkbox for the given property (with its value)
* CheckBoxFor(string id): Create an empty checkbox with the given id

### Html.Partial

* Partial(string path, object model = null): Include a cshtml page passing the given model.

### Html.RenderAction

* RenderAction(string action, string controller): Calls the controller passed as parameter with the given action, rendering the result directly included into the page. The corresponding action should be decorated with the __ChildActionOnly__ attribute. With this attribute it will not be invokable directly from the browser but only internally.