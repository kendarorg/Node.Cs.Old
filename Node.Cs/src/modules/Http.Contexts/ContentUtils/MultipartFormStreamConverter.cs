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

namespace Http.Contexts.ContentUtils
{
	public class MultipartFileDescriptor
	{
		public int Start;
		public int Length;
		public string Name;
		public string FileName;
		public string ContentType;
	}

	public class MultipartFormStreamConverter : BaseKeyValueStreamConverter
	{
		private const string BOUNDARY = "boundary=";
		private const string CONTENT_TYPE = "Content-Type:";
		private const string CONTENT_DISPOSITION = "Content-Disposition:";
		private const string CONTENT_START = "\r\n\r\n";
		private const string LINE_END = "\r\n";
		private const string CONTENT_END = "\r\n--";
		private const string FILENAME = "filename=";
		private const string NAME = "name=";

		private readonly List<MultipartFileDescriptor> _files = new List<MultipartFileDescriptor>();
		public IEnumerable<MultipartFileDescriptor> Files { get { return _files; } }

		private byte[] _boundaryBytes;
		private byte[] _contentTypeBytes;
		private byte[] _contentDispositionBytes;
		private byte[] _contentStartBytes;
		private byte[] _contentEndBytes;
		private byte[] _filenameBytes;
		private byte[] _nameBytes;
		private byte[] _lineEndBytes;

		public String ContentDisposition { get; private set; }

		private void InitializeFixedParts()
		{
			//Retrieve the boundary
			var boundaryIndex = IndexOf(_contentType, BOUNDARY) + BOUNDARY.Length;
			var boundary = _contentType.Substring(boundaryIndex, _contentType.Length - boundaryIndex);
			_boundaryBytes = _encoding.GetBytes(boundary);

			//Retrieve other bytes
			_contentTypeBytes = _encoding.GetBytes(CONTENT_TYPE);
			_contentDispositionBytes = _encoding.GetBytes(CONTENT_DISPOSITION);
			_contentStartBytes = _encoding.GetBytes(CONTENT_START);
			_contentEndBytes = _encoding.GetBytes(CONTENT_END);
			_filenameBytes = _encoding.GetBytes(FILENAME);
			_nameBytes = _encoding.GetBytes(NAME);
			_lineEndBytes = _encoding.GetBytes(LINE_END);
		}

		protected override void InitializeInternal()
		{
			InitializeFixedParts();

			var bytesBlockStart = -1;
			// ReSharper disable once TooWideLocalVariableScope
			int bytesBlockEnd;
			foreach (var match in ByteMatch.Matches(_content, _boundaryBytes))
			{
				if (bytesBlockStart == -1)
				{
					bytesBlockStart = match;
				}
				else
				{
					bytesBlockEnd = match;
					var blockLength = bytesBlockEnd - bytesBlockStart;
					if (blockLength > 0)
					{
						ParseSingleBlock(bytesBlockStart, blockLength);
					}
					bytesBlockStart = bytesBlockEnd;
				}
			}
		}

		private string GetString(int start, int end)
		{
			return _encoding.GetString(_content, start, end - start).Trim();
		}

		private void ParseSingleBlock(int startAt, int length)
		{
			var startContentDisposition = ByteMatch.IndexOf(_content, _contentDispositionBytes, startAt) + _contentDispositionBytes.Length;
			var endContentDisposition = ByteMatch.IndexOf(_content, (byte)';', startContentDisposition);

			var startName = ByteMatch.IndexOf(_content, _nameBytes, startAt, length) + _nameBytes.Length;
			startName = ByteMatch.IndexOf(_content, (byte)'\"', startName) + 1;
			var endName = ByteMatch.IndexOf(_content, (byte)'\"', startName);

			var startFileName = ByteMatch.IndexOf(_content, _filenameBytes, startAt, length);
			int endFileName = -1;
			int startContentType = -1;
			int endContentType = -1;
			if (startFileName > 0)
			{
				startFileName = ByteMatch.IndexOf(_content, (byte)'\"', startFileName) + 1;
				endFileName = ByteMatch.IndexOf(_content, (byte)'\"', startFileName);
				startContentType = ByteMatch.IndexOf(_content, _contentTypeBytes, startAt, length);
				startContentType = ByteMatch.IndexOf(_content, (byte)':', startContentType) + 1;
				endContentType = ByteMatch.IndexOf(_content, _lineEndBytes, startContentType);
			}
			var startContent = ByteMatch.IndexOf(_content, _contentStartBytes, startAt, length) + _contentStartBytes.Length;
			var endContent = startAt + length - _contentEndBytes.Length;

			var name = GetString(startName, endName);
			ContentDisposition = GetString(startContentDisposition, endContentDisposition);
			if (startFileName > 0)
			{
				var fileName = GetString(startFileName, endFileName);
				var contentType = GetString(startContentType, endContentType);
				_files.Add(new MultipartFileDescriptor
				{
					ContentType = contentType,
					FileName = fileName,
					Name = name,
					Start = startContent,
					Length = endContent - startContent
				});
			}
			else
			{
				var value = GetString(startContent, endContent);
				this[name] = value;
			}
		}

		private int IndexOf(string toCheck, string needle, int offset = 0, int count = 0)
		{
			if (count == 0)
			{
				return toCheck.IndexOf(needle, offset, StringComparison.OrdinalIgnoreCase);
			}
			return toCheck.IndexOf(needle, offset, count, StringComparison.OrdinalIgnoreCase);
		}
	}
}
