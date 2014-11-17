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
using System.Text;

namespace Http.Contexts.ContentUtils
{
	public abstract class BaseKeyValueStreamConverter : IRequestStreamConverter
	{
		private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		protected byte[] _content = { };
		protected Encoding _encoding;
		protected string _contentType;

		public byte[] Content { get { return _content; } }

		public string this[string key]
		{
			get
			{
				if (_parameters.ContainsKey(key)) return _parameters[key];
				return null;
			}
			set
			{
				_parameters.Add(key, value);
			}
		}
		public IEnumerable<string> Keys { get { return _parameters.Keys; } }

		protected abstract void InitializeInternal();

		public void Initialize(Stream body, Encoding encoding, String contentType)
		{
			var inputStream = new MemoryStream();
			using (inputStream) // here we have data
			{
				body.CopyTo(inputStream);
			}
			_contentType = contentType;
			_encoding = encoding;
			_content = inputStream.ToArray();
			InitializeInternal();
		}
	}
}
