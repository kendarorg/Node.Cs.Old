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
using CoroutinesLib.Shared.Logging;

namespace ConsoleLoggerModule
{

	public class ConsoleLogger : BaseLogger
	{
		private readonly bool _console;
		private readonly bool _debug;
		private readonly bool _trace;
		private ConsoleColor _color;

		public ConsoleLogger(LoggerLevel level, bool console = true, bool trace = false, bool debug = false) :
			base(level)
		{
			_console = console;
			_debug = debug;
			_trace = trace;
			_color = Console.BackgroundColor;
		}

		protected override void WriteLine(string toWrite, LoggerLevel level)
		{
			switch (level)
			{
				case (LoggerLevel.Fatal):
				case (LoggerLevel.Error):
					Console.BackgroundColor = ConsoleColor.Red;
					break;
				case (LoggerLevel.Warning):
					Console.BackgroundColor = ConsoleColor.Yellow;
					break;
				//case (LoggerLevel.Info):
				//case (LoggerLevel.Debug):
				default:
					Console.BackgroundColor = _color;
					break;
			}

			if (_console) Console.WriteLine(toWrite);
			if (_trace) Trace.WriteLine(toWrite);
#if DEBUG
			if (_debug) System.Diagnostics.Debug.WriteLine(toWrite);
#endif
		}
	}
}
