using System.Web.Razor;
using System.Web.Razor.Generator;

namespace Http.Renderer.Razor.Integration
{
	public class NodeRazorHost : RazorEngineHost
	{
		private GeneratedClassContext _generatedClassContext;

		public NodeRazorHost(RazorCodeLanguage codeLanguage)
			: base(codeLanguage)
		{
			//Override the default methods to use inside the template
			_generatedClassContext = new GeneratedClassContext("Execute", "Write", "WriteLiteral", "WriteTo", "WriteLiteralTo", typeof(HelperResult).FullName, "DefineSection")
			{
				ResolveUrlMethodName = "ResolveUrl",
			};
		}

		public override GeneratedClassContext GeneratedClassContext
		{
			get { return _generatedClassContext; }
			set { _generatedClassContext = value; }
		}
	}
}
