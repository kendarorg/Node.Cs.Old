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

namespace Node.Cs.Authorization
{
	public class DummyAuthenticationDataProvider : IAuthenticationDataProvider
	{
		public bool IsUserAuthorized(string user, string password)
		{
			return user == password;
		}
		public bool IsUserPresent(string userName)
		{
			return true;
		}


		public IUser CreateUser(string username, string password, string email, string passwordQuestion,
			string passwordAnswer, bool isApproved, object providerUserKey, out AuthenticationCreateStatus status)
		{
			if (username != password || string.IsNullOrWhiteSpace(password))
			{
				status = AuthenticationCreateStatus.InvalidPassword;
				return null;
			}
			if (string.IsNullOrWhiteSpace(username))
			{
				status = AuthenticationCreateStatus.InvalidUserName;
				return null;
			}
			status = AuthenticationCreateStatus.Success;
			return new DummyUser(username);
		}

		public IUser GetUser(string userName, bool isOnLine)
		{
			return new DummyUser(userName);
		}


		public void AddUsersToRoles(string[] userNames, string[] roleNames)
		{

		}


		public string[] GetUserRoles(string userName)
		{
			return new string[] { };
		}
	}

	public class DummyUser : IUser
	{
		public string _userName;
		public DummyUser(string userName)
		{
			_userName = userName;
		}
		public string UserName
		{
			get { return _userName; }
		}


		public bool ChangePassword(string oldPassword, string newPassword)
		{
			if (oldPassword != _userName) return false;
			if (string.IsNullOrWhiteSpace(oldPassword)) return false;
			return true;
		}

		public void SetRoles(params string[] roleNames)
		{

		}
	}
}
