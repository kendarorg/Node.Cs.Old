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


using System.Collections.Generic;
using System.Dynamic;
using Http.Shared.Controllers;

namespace HttpMvc.Controllers
{
	public class ViewResponse : IResponse, IViewResponse
	{
		public ModelStateDictionary ModelState { get; set; }
		public string View { get; set; }
		public object Model { get; set; }
		public dynamic ViewBag { get; set; }
		public Dictionary<string, object> ViewData { get; set; }

		public ViewResponse(string view, object model, dynamic viewBag = null)
		{
			ViewBag = viewBag ?? new ExpandoObject();
			View = view;
			Model = model;
		}
	}
}
