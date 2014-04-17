using System.Collections.Generic;
using ClassWrapper;

namespace Node.Cs.Lib.Controllers
{
	public interface IControllerWrapperInstance
	{
		IEnumerable<MethodWrapperDescriptor> GetMethodGroup(string action, string verb);
		ClassWrapper.ClassWrapper Instance { get; }
	}
}
