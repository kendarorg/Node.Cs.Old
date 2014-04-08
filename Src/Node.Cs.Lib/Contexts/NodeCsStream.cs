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
using System.IO;

namespace Node.Cs.Lib.Contexts
{
	public class NodeCsStream : Stream
	{
		public Action OnBeforeFlush { get; set; }
		private readonly Stream _s;

		public NodeCsStream(Stream baseStream)
		{
			_s = baseStream;
		}

		public override void Flush()
		{
			if (OnBeforeFlush != null)
			{
				OnBeforeFlush();
			}
			_s.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _s.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_s.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _s.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_s.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get { return _s.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _s.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _s.CanWrite; }
		}

		public override long Length
		{
			get { return _s.Length; }
		}

		public override long Position
		{
			get
			{
				return _s.Position;
			}
			set
			{
				_s.Position = value;
			}
		}

		public override void Close()
		{
			Flush();
			_s.Close();
		}
	}
}
