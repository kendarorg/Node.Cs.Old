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
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

namespace Node.Cs.TestHelpers
{
	public class MockStream : MemoryStream
	{
		public void Initialize()
		{
			Sw = new Stopwatch();
			ClosesCall = 0;
			WrittenBytes = 0;
			Seek(0, SeekOrigin.Begin);
			SetLength(0);
		}

		public Stopwatch Sw { get; private set; }
		public DateTime Start { get; private set; }
		public DateTime End { get; private set; }

		public int ClosesCall { get; private set; }
		public override void Close()
		{
			End = DateTime.Now;
			Sw.Stop();
			ClosesCall++;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (WrittenBytes == 0)
			{
				Start = DateTime.Now;
				Sw.Start();
			}
			WrittenBytes += count;
			base.Write(buffer, offset, count);
		}

		public int WrittenBytes { get; private set; }

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (WrittenBytes == 0)
			{
				Start = DateTime.Now;
				Sw.Start();
			}
			WrittenBytes += count;
			return base.WriteAsync(buffer, offset, count, cancellationToken);
		}
	}
}
