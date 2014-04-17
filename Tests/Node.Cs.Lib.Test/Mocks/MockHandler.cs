using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ConcurrencyHelpers.Caching;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Handlers;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Test
{
	public class MockHandler : Coroutine, IResourceHandler
	{
		public void Initialize(HttpContextBase context, PageDescriptor filePath, CoroutineMemoryCache memoryCache, IGlobalExceptionManager globalExceptionManager, PathProviders.IGlobalPathProvider globalPathProvider)
		{
			
		}

		public object Model { get; set; }

		public bool IsSessionCapable { get { return true; } }

		public dynamic ViewBag { get; set; }

		public Controllers.ModelStateDictionary ModelState { get; set; }

		public Dictionary<string, object> ViewData { get; set; }

		public override void OnError(Exception ex)
		{
			
		}

		public override IEnumerable<Step> Run()
		{
			yield break;
		}
	}
}
