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


using System.IO;
using System.Web;

namespace Http.Contexts.GenericUtils
{
	public class PostedFile : HttpPostedFileBase
	{
		private readonly string _fileName;
		private readonly Stream _stream;
		private readonly string _contentType;
		private readonly int _length;

		public PostedFile(string fileName, MemoryStream stream, string contentType)
		{
			_fileName = fileName;
			_stream = stream;
			_stream.Seek(0, SeekOrigin.Begin);
			_contentType = contentType;
			_length = (int)_stream.Length;
		}

		public PostedFile(HttpPostedFile httpPostedFile)
		{
			_fileName = httpPostedFile.FileName;
			_stream = httpPostedFile.InputStream;
			_contentType = httpPostedFile.ContentType;
			_length = httpPostedFile.ContentLength;
		}

		public override string FileName
		{
			get { return _fileName; }
		}

		public override Stream InputStream
		{
			get { return _stream; }
		}

		public override string ContentType
		{
			get { return _contentType; }
		}

		public override int ContentLength
		{
			get { return _length; }
		}

		public override void SaveAs(string filename)
		{
			var ms = _stream as MemoryStream;
			if (ms != null)
			{
				ms.Seek(0, SeekOrigin.Begin);
				File.WriteAllBytes(filename, ms.ToArray());
				return;
			}
			using (var streamReader = new MemoryStream())
			{
				InputStream.CopyTo(streamReader);
				File.WriteAllBytes(filename, streamReader.ToArray());
			}
		}
	}

}
