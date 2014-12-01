## Make a clone of the MVC4 Intranet Web Application with Node.Cs

### Setup the source

Create an MVC4 c# web application with the template "Intranet application". Name it IntranetWebApplicationMvc.

### Setup the Node.Cs server

Create a new class library IntranetWebApplicationNode. Remove the "Class1.cs" file.
Copy the following items from IntranetWebApplicationMvc into the root of the library

* App_Data/RouteConfig.cs
* Views
* Controllers
* Images
* Models
* Views
* Content
* Scripts
* favicon.ico
* Web.config

Through package manager add the libraries 

* node.cs.http
* node.cs.http.mvc

Modify the RouteConfig

* Let it inherit from "IRouteInitializer"
* Change the signature from "public static void RegisterRoutes(RouteCollection routes)" to "public void RegisterRoutes(IRoutingHandler routes)"
* Change "UrlParameter.Optional" to "RoutingParameter.Optional"

## Changes

Replace all "Controller" with "ControllerBase"
Replace all "ActionResult" with "IEnumerable&lt;IResponse&gt;""
Replace all "return" in controllers with "yield return"		
Let "ExternalLoginResult"	inherits from "IResponse"
Move all "yield return" out of try catch blocks

### Controllers\AccountController.cs

From

	try
	{
		WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
		yield return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
	}
	catch (Exception)
	{
		ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
	}

To

	IResponse result = null;
	try
	{
		WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
		result = RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
	}
	catch (Exception)
	{
		ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
	}
	if (result != null)
	{
		yield return result;
	}

### App_Start\RouteConfig.cs

UrlParameter.Optional => RoutingParameter.Optional
RouteCollection => IRoutingHandler

### Models\AccountModels.cs

* Table => System.ComponentModel.DataAnnotations.Schema.Table
* DatabaseGeneratedAttribute => System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute
* DatabaseGeneratedOption => System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption
* Key => System.ComponentModel.DataAnnotations.Key

And set the usings

* using System.Data.Entity;
* using NodeCs.Shared.Attributes;
* using NodeCs.Shared.Attributes.Validation;

### Controllers/AccountController.cs

Replace all "MembershipCreateStatus" with "AuthenticationCreateStatus"




