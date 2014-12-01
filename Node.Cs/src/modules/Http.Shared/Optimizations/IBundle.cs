using System.Collections.Generic;

namespace Http.Shared.Optimizations
{
	public interface IBundle
	{
		List<string> FisicalAddresses { get; }
		string LogicalAddress { get; }
	}
}