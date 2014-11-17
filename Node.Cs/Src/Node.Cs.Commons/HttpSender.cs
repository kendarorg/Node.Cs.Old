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


#if TESTHTTP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ConcurrencyHelpers.Coroutines;
using ConcurrencyHelpers.Utils;

namespace Node.Cs.Lib
{
	public class HttpSender : CoroutineThread
	{
		private static int senders = Environment.ProcessorCount;
		private static CounterInt64 counter = new CounterInt64();
		private static HttpSender[] _senders;

		static HttpSender()
		{
			var list = new List<HttpSender>();
			for (int i = 0; i < senders; i++)
			{
				list.Add(new HttpSender());
				list.Last().Start();
			}
			_senders = list.ToArray();
		}

		private HttpSender()
			: base(10, -1)
		{

		}
		public static void Send(Stream stream, byte[] bytes, bool close)
		{
			counter++;
			var current = counter.Value%_senders.Length;
			_senders[(int)current].AddCoroutine(new StreamCoroutine(stream, bytes, close));
		}
	}

	public class StreamCoroutine : Coroutine
	{
		private Stream stream;
		private byte[] bytes;
		private bool close;

		public StreamCoroutine(Stream stream, byte[] bytes, bool close)
		{
			// TODO: Complete member initialization
			this.stream = stream;
			this.bytes = bytes;
			this.close = close;
		}
		public override void OnError(System.Exception ex)
		{
			ShouldTerminate = true;
		}

		public override System.Collections.Generic.IEnumerable<Step> Run()
		{
			stream.Write(bytes, 0, bytes.Length);
			if (close)
			{
				stream.Close();
			}
			yield break;
		}
	}
}
#endif