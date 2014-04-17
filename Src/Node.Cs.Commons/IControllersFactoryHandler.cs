
namespace Node.Cs.Lib
{
	public interface IControllersFactoryHandler
	{
		object Create(string p);
		void Release(Controllers.IController controller);
		bool SupportSession(string p);
		void Initialize(string p);
	}
}
