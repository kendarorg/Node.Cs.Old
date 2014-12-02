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


using NodeCs.Shared;
using System;

namespace Http.Shared.Authorization
{
	public static class WebSecurity
	{
		public static bool Login(string userName, string password, bool persistCookie = false)
		{
			if (persistCookie)
			{
				throw new NotImplementedException();
			}
			var adp = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();

			return adp.IsUserAuthorized(userName, password);
		}

		public static void Logout()
		{
			throw new NotImplementedException();
		}

		public static void CreateUserAndAccount(string userName, string password, Object propertyValues = null, bool requireConfirmationToken = false)
		{
			if (requireConfirmationToken)
			{
				throw new NotImplementedException();
			}
			var adp = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();
			AuthenticationCreateStatus result;
			adp.CreateUser(userName, password, string.Empty, string.Empty, Guid.NewGuid().ToString(), true, null, out result);
			if (result != AuthenticationCreateStatus.Success)
			{
				throw new MembershipCreateUserException(userName, result);
			}
		}

		public static int GetUserId(string name)
		{
			var adp = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();
			var user = adp.GetUser(name, false);
			return user.UserId;
		}

		public static bool ChangePassword(string name, string oldPassword, string newPassword)
		{
			var adp = ServiceLocator.Locator.Resolve<IAuthenticationDataProvider>();
			var user = adp.GetUser(name, false);
			return user.ChangePassword(oldPassword, newPassword);
		}

		public static void CreateAccount(string name, string newPassword, bool requireConfirmationToken = false)
		{
			CreateUserAndAccount(name, newPassword, requireConfirmationToken);
		}
	}
}
