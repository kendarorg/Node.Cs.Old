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

namespace Node.Cs.Lib.Loggers
{
	public class ConsoleLogger : ILogger
	{
		private string Now()
		{
			return DateTime.UtcNow.ToString("s") + " ";
		}

		public bool IsErrorEnabled { get; set; }

		public void Error(string message, params object[] pars)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(Now() + Logger._ERROR + " " + message, pars);
		}

		public void Error(Exception ex, string message = null, params object[] pars)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(Now() + Logger._ERROR + " " + message + "\n" + ex, pars);
		}
		
		public bool IsWarningEnabled { get; set; }

		public void Warn(string message, object[] pars)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(Now() + Logger._WARNING + " " + message, pars);
		}


		public bool IsInfoEnabled { get; set; }

		public void Info(string message, object[] pars)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(Now() + Logger._INFO + " " + message, pars);
		}
	}
}
