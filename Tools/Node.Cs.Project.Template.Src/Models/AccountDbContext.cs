using System.Data.Entity;
using Node.Cs.EntityFramework;
using Node.Cs.EntityFramework.Security;

namespace Node.Cs.Project.Template.Src.Models
{
	[DbConfigurationType(typeof(NodeCsDbConfiguration))]
	public class AccountDbContext : EfSecurityDdContext
	{
		public AccountDbContext()
			: base(EfInitializer.ConnectionString)
		{

		}
	}
}
