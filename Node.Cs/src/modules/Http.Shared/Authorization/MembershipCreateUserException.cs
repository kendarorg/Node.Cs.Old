using System;

namespace Http.Shared.Authorization
{
	public class MembershipCreateUserException : Exception
	{
		public MembershipCreateUserException(String message, AuthenticationCreateStatus status = AuthenticationCreateStatus.ProviderError)
			: base(message)
		{
			StatusCode = status;
		}
		public AuthenticationCreateStatus StatusCode { get; private set; }
	}
}