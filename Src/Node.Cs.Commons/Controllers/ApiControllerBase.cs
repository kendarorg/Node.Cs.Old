// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using Node.Cs.Lib.Utils;

namespace Node.Cs.Lib.Controllers
{
	public abstract class ApiControllerBase : MainControllerBase
	{
		
	}

	public abstract class MainControllerBase : IController
	{
		public ModelStateDictionary ModelState { get; set; }

		private HttpContextBase _context;

		public HttpContextBase HttpContext
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
				Url = new UrlHelper(_context);
			}
		}

		public IResponse JsonResponse(object obj, string contentType = null, Encoding encoding = null)
		{
			var res = new JsonResponse();
			res.Initialize(obj, contentType, encoding);
			return res;
		}

		public IResponse TextResponse(string obj, string contentType = null, Encoding encoding = null)
		{
			var res = new StringResponse();
			res.Initialize(obj, contentType, encoding);
			return res;
		}

		public IResponse XmlResponse(object obj, string contentType = null, Encoding encoding = null)
		{
			var res = new XmlResponse();
			res.Initialize(obj, contentType, encoding);
			return res;
		}

		public IResponse ByteResponse(byte[] obj, string contentType = null, Encoding encoding = null)
		{
			var res = new ByteResponse();
			res.Initialize(obj, contentType, encoding);
			return res;
		}

		public UrlHelper Url { get; private set; }

		public IPrincipal User
		{
			get
			{
				return _context.User;
			}
		}

		public static IResponse InvokeLocalAndWait<T>(Func<IEnumerable<T>> func, Container result = null)
		{
			var step = Coroutine.InvokeLocalAndWait(func, result);
			return new StepResponse(step);
		}

		public static IResponse InvokeAsTaskAndWait(Action action)
		{
			var step = Coroutine.InvokeAsTaskAndWait(action);
			return new StepResponse(step);
		}

		public static IResponse InvokeTaskAndWait(Task task)
		{
			var step = Coroutine.InvokeTaskAndWait(task);
			return new StepResponse(step);
		}

		public static IResponse InvokeCoroutineAndWait(CoroutineThread thread, ICoroutine action, bool adding = true)
		{
			throw new NotImplementedException();
			/*var step = Coroutine.InvokeCoroutineAndWait(func, result);
			return new StepResponse(step);*/
		}

		public static object CallCoroutine(IEnumerable<Step> coroutineEnumerable)
		{
			return Coroutine.CallCoroutine(coroutineEnumerable);
		}

		protected bool TryUpdateModel(Object model)
		{
			var requestContentType = HttpContext.Request.ContentType;
			if (!GlobalVars.ConversionService.HasConverter(requestContentType)) return false;
			var convertedModel = GlobalVars.ConversionService.Convert(model.GetType(), requestContentType, HttpContext.Request);
			var old = ValidationAttributesService.GetWrapper(model);
			var converted = ValidationAttributesService.GetWrapper(convertedModel);

			foreach (var prop in converted.Properties)
			{
				var newProp = converted.GetObject(prop);
				if (newProp != null)
				{
					old.Set(prop, newProp);
				}
			}

			if (!ValidationAttributesService.CanValidate(model)) return false;
			return ValidationAttributesService.ValidateModel(model, ModelState);
		}
	}
}