using System.Collections.Generic;

namespace Node.Cs.Lib.Contexts
{
	public interface INodeCsSession
	{
		string SessionID { get; }
		Dictionary<string, object> SessionData { get; }
		bool IsChanged { get; }
		bool IsCleared { get; }
	}
}
