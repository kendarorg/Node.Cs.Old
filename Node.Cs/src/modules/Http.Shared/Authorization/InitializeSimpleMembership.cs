using System;
using Http.Shared.Contexts;

namespace Http.Shared.Authorization
{
	public class InitializeSimpleMembership : Attribute, IFilter
	{
		public bool OnPreExecute(IHttpContext context)
		{
			return true;
		}

		public void OnPostExecute(IHttpContext context)
		{
			
		}
	}
}