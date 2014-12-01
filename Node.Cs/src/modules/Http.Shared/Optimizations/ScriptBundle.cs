using System.Collections.Generic;

namespace Http.Shared.Optimizations
{
	public class ScriptBundle : IBundle
	{
		public List<string> FisicalAddresses { get; private set; }
		public string LogicalAddress { get; private set; }

		public ScriptBundle(string logicalAddress)
		{
			LogicalAddress = logicalAddress;
			FisicalAddresses = new List<string>();
		}

		public ScriptBundle Include(params string[] fisicalAddresses)
		{
			foreach (var fisicalAddress in fisicalAddresses)
			{
				FisicalAddresses.Add(fisicalAddress);
			}
			return this;
		}
	}
}