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


using System.Collections.Generic;
using System.Linq;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Authorization
{
	public class JsonUser : IUser
	{
		public JsonUser()
		{

		}

		private JsonAuthenticationDataProvider _dataProvider;
		public void SetDataProvider(JsonAuthenticationDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public JsonUser(string userName, string password)
		{
			UserName = userName;
			Password = password;
			Roles = new List<string>();
		}


		public int Id { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
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

	public class JsonAuthenticationDataProvider : IAuthenticationDataProvider
	{
		private const string USERS = "UsersData";
		public bool IsUserAuthorized(string user, string password)
		{
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (data.UserName == user && data.Password == password)
				{
					return true;
				}
			}
			return false;
		}

		public IUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
			bool isApproved, object providerUserKey, out AuthenticationCreateStatus status)
		{
			status = AuthenticationCreateStatus.Success;
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (data.UserName == username)
				{
					status = AuthenticationCreateStatus.DuplicateUserName;
					return null;
				}
			}
			var user = new JsonUser(username, password);
			user.SetDataProvider(this);

			var id = JsonDataService.ReserveId(USERS);
			user.Id = id;
			JsonDataService.SaveData(USERS, user, id);
			return user;
		}

		public bool IsUserPresent(string userName)
		{
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (data.UserName == userName)
				{
					return true;
				}
			}
			return false;
		}

		public IUser GetUser(string userName, bool isOnLine)
		{
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (data.UserName == userName)
				{
					data.SetDataProvider(this);
					return data;
				}
			}
			return null;
		}

		internal bool ChangePassword(JsonUser user, string oldPassword, string newPassword)
		{
			if (oldPassword != user.Password) return false;
			user.Password = newPassword;
			JsonDataService.SaveData(USERS, user, user.Id);
			return true;
		}


		public void AddUsersToRoles(string[] userNames, string[] roleNames)
		{
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (userNames.Any(u => u == data.UserName))
				{
					data.SetRoles(roleNames);
				}
			}
		}


		public string[] GetUserRoles(string userName)
		{
			foreach (var data in JsonDataService.ReadData<JsonUser>(USERS))
			{
				if (data.UserName == userName)
				{
					return data.Roles.ToArray();
				}
			}
			return new string[] { };
		}
	}
}
