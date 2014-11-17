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
using System.Security.Principal;
using System.Text;
using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using Http.Shared.Routing;
using NodeCs.Shared;

namespace HttpMvc.Controllers
{
	public abstract class MainControllerBase : IController
	{
		public ModelStateDictionary ModelState { get; set; }

		
		public IHttpContext HttpContext { get; set; }

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

		public UrlHelper Url { get; set; }

		public IConversionService ConversionService { get; set; }

		public IPrincipal User
		{
			get
			{
				return HttpContext.User;
			}
		}

		protected bool TryUpdateModel(Object model)
		{
			var requestContentType = HttpContext.Request.ContentType;
			if (!ConversionService.HasConverter(requestContentType)) return false;
			var convertedModel = ConversionService.Convert(model.GetType(), requestContentType, HttpContext.Request);
			var old = ValidationService.GetWrapper(model);
			var converted = ValidationService.GetWrapper(convertedModel);

			foreach (var prop in converted.Properties)
			{
				var newProp = converted.GetObject(prop);
				if (newProp != null)
				{
					old.Set(prop, newProp);
				}
			}

			if (!ValidationService.CanValidate(model)) return false;
			var result = ValidationService.ValidateModel(model);
			foreach (var item in result)
			{
				ModelState.AddModelError(item.Property,item.Message);
			}
			return ModelState.IsValid;
		}
	}
}