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
using System.IO;
using System.Web;

namespace Node.Cs.Lib.Utils
{
	public class ConversionService : IConversionService
	{
		private readonly Dictionary<string, ISerializer> _mimeConverters;

		
		public  ConversionService()
		{
			_mimeConverters = new Dictionary<string, ISerializer>(StringComparer.OrdinalIgnoreCase);
		}

		public void AddConverter(ISerializer converter, string firstMime, params string[] mimes)
		{
			AddConverter(firstMime, converter);
			foreach (var mime in mimes)
			{
				AddConverter(mime, converter);
			}
		}

		private void AddConverter(string mime, ISerializer converter)
		{
			if (_mimeConverters.ContainsKey(mime))
			{
				_mimeConverters.Remove(mime);
			}
			_mimeConverters.Add(mime, converter);
		}

		public bool HasConverter(string mime)
		{
			if (string.IsNullOrWhiteSpace(mime)) return false;
			return _mimeConverters.ContainsKey(mime);
		}

		public T Convert<T>(string mime, HttpRequestBase request)
		{
			if (!HasConverter(mime)) return default(T);
			return (T)Convert(typeof(T), mime, request);
		}

		public object Convert(Type t, string mime, HttpRequestBase request)
		{
			if (!HasConverter(mime)) return null;
			return _mimeConverters[mime].Deserialize(t, request);
		}

		public byte[] Convert<T>(string mime, T src)
		{
			if (!HasConverter(mime)) return new byte[] { };
			return Convert(typeof(T), mime, src);
		}

		public byte[] Convert(Type t, string mime, object src)
		{
			if (!HasConverter(mime)) return new byte[] { };
			return _mimeConverters[mime].Serialize(t, src);
		}
	}
}
