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


using Http.Shared;
using Http.Shared.Contexts;
using Http.Shared.Controllers;
using HttpMvc.Controllers;
using NodeCs.Shared;

namespace HttpMvc
{
	public class DataResponseHandler:IResponseHandler
	{
		public void Handle(IHttpContext context, IResponse response)
		{
			var filtersHandler = ServiceLocator.Locator.Resolve<IFilterHandler>();
			var dataResponse = (DataResponse) response;
			filtersHandler.OnPostExecute(context);
			context.Response.ContentEncoding = dataResponse.ContentEncoding;
			context.Response.ContentType = dataResponse.ContentType;
			context.Response.OutputStream.WriteAsync(dataResponse.Data, 0, dataResponse.Data.Length)
				.ContinueWith((a) => context.Response.Close());
		}

		public bool CanHandle(IResponse response)
		{
			return response as DataResponse !=null;
		}
	}
}
