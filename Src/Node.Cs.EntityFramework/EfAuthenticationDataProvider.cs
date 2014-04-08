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


using System;
using System.Collections.Generic;
using System.Linq;
using Node.Cs.Authorization;
using Node.Cs.EntityFramework.Security;

namespace Node.Cs.EntityFramework
{
	public class EfUser : IUser
	{
		public EfUser()
		{

		}

		private EfAuthenticationDataProvider _dataProvider;

		public void SetDataProvider(EfAuthenticationDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public EfUser(string userName, string eMail)
		{
			UserName = userName;
			Email = eMail;
			Roles = new List<string>();
		}


		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public List<string> Roles { get; set; }

		public bool ChangePassword(string oldPassword, string newPassword)
		{
			return _dataProvider.ChangePassword(this, oldPassword, newPassword);
		}

		public void SetRoles(params string[] roleNames)
		{
			Roles = new List<string>(roleNames);
		}
	}

	public class EfAuthenticationDataProvider : IAuthenticationDataProvider
	{
		private static Func<EfSecurityDdContext> _getDbContext;

		public EfAuthenticationDataProvider(Func<EfSecurityDdContext> getDbContext)
		{
			_getDbContext = getDbContext;
		}

		public bool IsUserAuthorized(string user, string password)
		{
			var dbc = _getDbContext();
			return dbc.UserProfiles.Any(a => a.UserName == user && a.UserPassword == password && a.IsApproved);
		}

		public IUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
			bool isApproved, object providerUserKey, out AuthenticationCreateStatus status)
		{
			status = AuthenticationCreateStatus.Success;
			var dbc = _getDbContext();
			if (dbc.UserProfiles.Any(a => a.UserName == username))
			{
				status = AuthenticationCreateStatus.DuplicateUserName;
				return null;
			}

			var newUser = new UserProfile();
			newUser.UserName = username;
			newUser.UserPassword = password;
			newUser.PasswordQuestion = passwordQuestion;
			newUser.PasswordAnswer = passwordAnswer;
			newUser.IsApproved = isApproved;

			dbc.UserProfiles.Add(newUser);
			dbc.SaveChanges();


			var result = new EfUser(newUser.UserName, newUser.Email);
			result.SetDataProvider(this);
			return result;
		}

		public bool IsUserPresent(string userName)
		{
			using (var dbc = _getDbContext())
			{
				return dbc.UserProfiles.Any(a => a.UserName == userName);
			}
		}

		public IUser GetUser(string userName, bool isOnLine)
		{
			using (var dbc = _getDbContext())
			{
				var user = dbc.UserProfiles.FirstOrDefault(a => a.UserName == userName);
				if (user == null) return null;
				var outUser= new EfUser(user.UserName, user.Email);
				outUser.SetDataProvider(this);
				return outUser;
			}
		}

		internal bool ChangePassword(EfUser user, string oldPassword, string newPassword)
		{
			using (var dbc = _getDbContext())
			{
				var realUser = dbc.UserProfiles.FirstOrDefault(a => a.UserName == user.UserName);
				if (realUser == null) return false;
				if (oldPassword != realUser.UserPassword) return false;
				realUser.UserPassword = newPassword;
				dbc.SaveChanges();
				return true;
			}
		}

		public void AddUsersToRoles(string[] userNames, string[] roleNames)
		{
			using (var dbc = _getDbContext())
			{
				var roles =
					dbc.UserRoles.Where(r => roleNames.Any(rn => string.Compare(rn, r.Role, StringComparison.OrdinalIgnoreCase) == 0))
						.ToArray();

				foreach (var userName in userNames)
				{
					var user = dbc.UserProfiles.FirstOrDefault(a => a.UserName == userName);
					if (user == null) continue;
					user.Roles.Clear();
					foreach (var role in roles)
					{
						user.Roles.Add(role);
					}
				}
				dbc.SaveChanges();
			}
		}


		public string[] GetUserRoles(string userName)
		{
			using (var dbc = _getDbContext())
			{
				var realUser = dbc.UserProfiles.FirstOrDefault(a => a.UserName == userName);
				if (realUser == null) return new string[] {};
				return realUser.Roles.Select(r => r.Role).ToArray();
			}
		}
	}
}
