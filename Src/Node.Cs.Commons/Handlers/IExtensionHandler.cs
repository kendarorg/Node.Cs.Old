
using System.Collections.ObjectModel;
using System.Web;
using ConcurrencyHelpers.Coroutines;

namespace Node.Cs.Lib.Handlers
{
	public interface IExtensionHandler
	{
		ICoroutine CreateInstance(HttpContextBase httpContextBase, PageDescriptor foundedPath);
		void InitializeHandlers();
		ReadOnlyCollection<string> Extensions { get; }
		bool IsSessionCapable(string foundedExt);
	}
}
