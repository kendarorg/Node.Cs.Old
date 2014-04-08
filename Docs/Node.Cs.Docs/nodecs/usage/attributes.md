<!--settings(
title=Attributes and filters
description=Attributes and filters.
)-->

## {Title}

Several attributes are taken from the standard Asp.NET MVC

### Request type attributes

* HttpPost: The controller method respond to POST
* HttpGet: The controller method respond to GET
* HttpDelete: The controller method respond to DELETE
* HttpPut: The controller method respond to PUT
* ActionName: The following function will be invoke only when the action will be equals to the action name.

### Validation and Scaffolding

These attributes are used for the automatic validation of the fields. They work exactly as in Asp.Net MVC

* Required: The field is required
* StringLength: Specify the min/max length of the field
* Range: Specify the min/max value of the numeric data
* RegularExpression: Validate against a regex

The following attributes are used with the scaffolding mechanism. They work exactly as in Asp.Net MVC

* DataType: Create a field that is spcific for the given data type (e.g. email, password etc). This will be used for the DisplayFor,EditorFor and EditorForModel methods
* Display/DisplayName: Specify the label to show for the propery
* ScaffoldColumn: If set to false the property will not be created in the scaffolding inside the EditorForModel.

### Authorization

Some filter for authorization and security is present. They work exactly as in Asp.Net MVC

* ShouldBeLocal: Requests allowed only from localhost on the controller
* Authorize: Verify the user credentials. Can specify the rolse that are allowed
