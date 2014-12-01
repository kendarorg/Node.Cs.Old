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
