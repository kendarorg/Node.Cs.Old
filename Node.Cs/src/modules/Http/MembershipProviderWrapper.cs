using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.Security;
using Http.Shared.Authorization;

namespace Http
{
	public class MembershipProviderWrapper : MembershipProvider
	{
		private readonly IAuthenticationDataProvider _authenticationDataProvider;

		public MembershipProviderWrapper(IAuthenticationDataProvider authenticationDataProvider)
		{
			_authenticationDataProvider = authenticationDataProvider;
			Initialize(authenticationDataProvider.GetType().Name,new NameValueCollection());
		}

		

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
			bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			AuthenticationCreateStatus statusBase;
			var result = _authenticationDataProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer,
				isApproved, providerUserKey, out statusBase);
			status = (MembershipCreateStatus) statusBase;
			return new MembershipUser(
				_authenticationDataProvider.GetType().Name,
				result.UserName,
				result.UserId,null,null,null,true,false,DateTime.UtcNow,DateTime.UtcNow,DateTime.UtcNow,DateTime.UtcNow,DateTime.UtcNow);
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
			string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			var res = _authenticationDataProvider.GetUser(username, false);
			return res.ChangePassword(oldPassword, newPassword);
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public override bool ValidateUser(string username, string password)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			var result = _authenticationDataProvider.GetUser(username, false);
			return new MembershipUser(
				_authenticationDataProvider.GetType().Name,
				result.UserName,
				result.UserId, null, null, null, true, false, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			return _authenticationDataProvider.DeleteUser(username, deleteAllRelatedData);
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}

		public override string ApplicationName { get; set; }

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}
	}
}
