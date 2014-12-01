using System.Collections.Generic;

namespace Http.Shared.Optimizations
{
	public class StyleBundle : IBundle
	{
		public List<string> FisicalAddresses { get; private set; }
		public string LogicalAddress { get; private set; }

		public StyleBundle(string logicalAddress)
		{
			LogicalAddress = logicalAddress;
			FisicalAddresses = new List<string>();
		}

		public StyleBundle Include(params string[] fisicalAddresses)
		{
			foreach (var fisicalAddress in fisicalAddresses)
			{
				FisicalAddresses.Add(fisicalAddress);
			}
			return this;
		}
	}
}