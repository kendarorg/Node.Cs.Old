using System;
using Http.Shared.Contexts;

namespace Http.Shared.Authorization
{
	public class AllowAnonymous : Attribute, IFilter
	{
		public bool OnPreExecute(IHttpContext context)
		{
			return true;
		}

		public void OnPostExecute(IHttpContext context)
		{

		}
	}

	public class ValidateAntiForgeryToken : Attribute, IFilter
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