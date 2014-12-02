using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Renderer.Razor.Integration
{
	public interface ILiteralString { }
	public class HelperResult : ILiteralString
	{
		private readonly Action<TextWriter> _action;

		public HelperResult(Action<TextWriter> action)
		{
			if (action == null) throw new ArgumentNullException("action");
			_action = action;
		}

		public override string ToString()
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				_action(writer);
				return writer.ToString();
			}
		}


	}
}
