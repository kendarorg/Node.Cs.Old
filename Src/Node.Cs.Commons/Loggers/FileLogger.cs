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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrencyHelpers.Containers;
using ConcurrencyHelpers.Interfaces;
using ConcurrencyHelpers.Utils;

namespace Node.Cs.Lib.Loggers
{
	public class FileLogger : ILogger
	{
		private readonly string _filePath;
		private readonly SystemTimer _timer;
		private readonly LockFreeQueue<string> _messages;
		private bool _overlapping;

		public FileLogger(string filePath = "FileLogger")
		{
			var now = DateTime.Now.ToString("yyyyMMdd.HHmmss");
			_filePath = filePath + "." + now + ".log";
			File.WriteAllText(_filePath, "Starting\n");
			_overlapping = false;
			_messages = new LockFreeQueue<string>();
			_timer = new SystemTimer(500);
			_timer.Elapsed += OnElapsed;
			_timer.Start();


		}

		void OnElapsed(object sender, ElapsedTimerEventArgs e)
		{
			if (_messages.Count == 0) return;
			if (_overlapping) return;
			_overlapping = true;
			try
			{
				File.AppendAllLines(_filePath, _messages.Dequeue(1000));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				_overlapping = false;
			}
		}
		private string Now()
		{
			return DateTime.UtcNow.ToString("s") + " ";
		}

		public bool IsErrorEnabled { get; set; }
		public bool IsWarningEnabled { get; set; }

		public void Error(string message, params object[] pars)
		{
			_messages.Enqueue(string.Format(Now() + Logger._ERROR + " " + message, pars));
		}

		public void Error(Exception ex, string message = null, params object[] pars)
		{
			_messages.Enqueue(string.Format(Now() + Logger._ERROR + " " + message + "\r\n" + ex, pars));
		}

		public void Warn(string message, object[] pars)
		{
			_messages.Enqueue(string.Format(Now() + Logger._WARNING + " " + message, pars));
		}


		public bool IsInfoEnabled { get; set; }

		public void Info(string message, object[] pars)
		{
			_messages.Enqueue(string.Format(Now() + Logger._INFO + " " + message, pars));
		}
	}
}
