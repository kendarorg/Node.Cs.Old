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


using Node.Cs.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Node.Cs.Lib.Conversions
{
	public class FormNodeCsSerializer : ISerializer
	{
		public object Deserialize(Type t, HttpRequestBase request, Encoding encoding = null)
		{
			var jsonFromRequest = MakeJsonFromRequestParams(request);
			var jsSerializer = new JavaScriptSerializer();
			return jsSerializer.Deserialize(jsonFromRequest, t);
		}

		public byte[] Serialize(Type t, object src, Encoding encoding = null)
		{
			throw new NotSupportedException();
		}

		private string MakeJsonFromRequestParams(HttpRequestBase request)
		{
			var jf = new JsonForm();
			foreach (var key in request.Params.AllKeys)
			{
				jf.Add(key, request.Params[key]);
			}
			return jf.ToJson();
		}


	}

	internal class JsonForm
	{
		private readonly List<string> _items = new List<string>();
		public string ToJson()
		{
			return "{" + string.Join(",", _items) + "}";
		}


		public void Add(string key, string value)
		{
			var firstDot = key.IndexOf('.');
			string next = null;
			string currentField = null;
			if (firstDot > 0)
			{
				currentField = key.Substring(0, firstDot);
				next = key.Substring(firstDot + 1);
				throw new NotImplementedException("Dot notation in forms not yet supported " + key);
			}
			else if (firstDot < 0)
			{
				currentField = key;
			}
			else
			{
				throw new NotImplementedException("Notation in forms not yet supported " + key);
			}
			_items.Add("\"" + key + "\":\"" + value.Replace("\"", "\\\"") + "\"");
		}
	}
}
