<!--settings(
title=Security and authorization
description=Security and authorization.
)-->

## {Title}

The Node.Cs server offer two types of authentication methods, all compatible with the standard
implementation of Asp.NET MVC:

* Form
* Basic

### Configuration

The configuration must be set on the node.config file into the Security section. The parameters are

* LoginPage: The route that will be used to login.
* AuthenticationType: Form or Basic
* Realm: The realm that will be used internally for the authentication.

<pre class="brush:html;">
<NodeCsConfiguration>
	<NodeCsSettings>
		<Security>
			<LoginPage>/Account/LogOn</LoginPage>
			<AuthenticationType>Form</AuthenticationType>
			<Realm>Node.Cs</Realm>
		</Security>
			...
</pre>

Other than this the GlobalNodeCs.cs must be modified to specify what service will verify the 
authentication data, and to add the GlobalAuthorize filter to handle all authorization requests.

This can be made inside the Application_Start method:

<pre class="brush: csharp;">
public void Application_Start()
{
	...
	GlobalVars.AuthenticationDataProvider = new EfAuthenticationDataProvider(() => new MusicStoreEntities());
	GlobalVars.GlobalFilters.Add(new GlobalAuthorize());
	...
</pre>

For this example we are using the EfAuthenticationDataProvider, a database dataprovider based on EntityFramework.
Then we add the filter to the global filters.

### Usage

Inside the NodeCsMusicStore sample all is written clearly but just to give an example with the Form
authentication type

#### Authorize methods or controllers

A specific "Authorize" attribute as in Asp.NET MVC should be added to the methods or controller needing
to be secured. For the controller

<pre class="brush: csharp;">
[Authorize(Roles = "Administrator")]
public class StoreManagerController : ControllerBase, IDisposable
{
</pre>

Or for the method

<pre class="brush: csharp;">
	[Authorize]
	public IEnumerable<IResponse> Index()
	{
</pre>

If not authorized the attribute will redirect to the LogOn page specified inside the settings and
inside the controller properties, the property "User" will be set with the current user.

__REMEMBER TO ADD THE GlobalAuthorize FILTER!!!__

#### Registration

The process is simple, after verifyng that the ModelState has no errors (this check the attributes on the model
class).

* Take an instance of the AuthenticationDataProvider from the global variables
* Call the create user function with the required parameters (in this case username, password and email)
* Check the createStatus, if it is correct initialize the authorization cookie and redirect to the home page
* If some problem happened, redisplay the page with the same model.
* Rolse should be added at this point!

<pre class="brush: csharp;">
[HttpPost]
public IEnumerable<IResponse> Register(RegisterModel model)
{
	if (ModelState.IsValid)
	{
		var authProvider = GlobalVars.AuthenticationDataProvider;

		// Attempt to register the user
		AuthenticationCreateStatus createStatus;
		var user = authProvider.CreateUser(model.UserName, model.Password, model.Email, 
			null, null, true, null, out createStatus);

		if (createStatus == AuthenticationCreateStatus.Success)
		{
			authProvider.AddUsersToRoles(new[] { user.UserName }, new[] { "Administrator" });
			Authorize.FormSetAuthCookie(HttpContext, model.UserName, false);
			yield return RedirectToAction("Index", "Home");
		}
		else
		{
			ModelState.AddModelError("", "Unable to create user");
		}
	}

	// If we got this far, something failed, redisplay form
	yield return View(model);
}
</pre>

#### LogOn

The procedure is more or less the same

* Retrieve the AuthenticationDataProvider
* Check if the user is authorized
* If it's authorized go back to the url that required the logon screen or if not present, on the home and set the authorization cookie
* Else show the errors on the current view

<pre class="brush: csharp;">
[HttpPost]
public IEnumerable<IResponse> LogOn(LogOnModel model, string returnUrl)
{
	if (ModelState.IsValid)
	{
		var authProvider = GlobalVars.AuthenticationDataProvider;
		if (authProvider.IsUserAuthorized(model.UserName, model.Password))
		{
			Authorize.FormSetAuthCookie(HttpContext, model.UserName, model.RememberMe);
			if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
					&& !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
			{
				yield return Redirect(returnUrl);
			}
			else
			{
				yield return RedirectToAction("Index", "Home");
			}
		}
		else
		{
			ModelState.AddModelError("", "The user name or password provided is incorrect.");
		}
	}

	// If we got this far, something failed, redisplay form
	yield return View(model);
}
</pre>

#### LogOff

Simply execute the appropriate signout method for the authorization required

<pre class="brush: csharp;">
public IEnumerable<IResponse> LogOff()
{
	Authorize.FormSignOut(HttpContext);

	yield return RedirectToAction("Index", "Home");
}
</pre>

### Custom security providers

To have a custom provider for authorization, some interface must be implemented:

IUser, that represents the user. Note that the user MUST know what is its data provider
to do the ChangePassword and SetRoles operations

<pre class="brush: csharp;">
	public interface IUser
	{
		string UserName { get; }
		bool ChangePassword(string oldPassword, string newPassword);
		void SetRoles(params string[] roleNames);
	}
</pre>

And the data provider itself. Note that there will be __ONE AND ONLY ONE INSTANCE__ of this
class so, be carefule adding instance variables cheking that the will be thread-safe!!

<pre class="brush: csharp;">
	public interface IAuthenticationDataProvider
	{
		bool IsUserAuthorized(string user, string password);
		IUser CreateUser(string username, string password, string email, string passwordQuestion,
		string passwordAnswer, bool isApproved, object providerUserKey, out AuthenticationCreateStatus status);
		bool IsUserPresent(string userName);
		IUser GetUser(string userName, bool isOnLine);
		void AddUsersToRoles(string[] userNames, string[] roleNames);
		string[] GetUserRoles(string p);
	}
</pre>