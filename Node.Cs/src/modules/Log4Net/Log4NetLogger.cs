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


using CoroutinesLib.Shared.Logging;
using log4net;
using System;
using System.Reflection;

namespace NodeLog4Net
{
	public class Log4NetLog : BaseLogger
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected override void LogException(Exception exception, string format, object[] pars, LoggerLevel level)
		{
			var toWrite = string.Format(format, pars);
			switch (level)
			{
				case (LoggerLevel.Fatal):
					_log.Fatal(toWrite, exception);
					break;
				case (LoggerLevel.Error):
					_log.Error(toWrite, exception);
					break;
				case (LoggerLevel.Warning):
					_log.Warn(toWrite, exception);
					break;
				case (LoggerLevel.Info):
					_log.Info(toWrite, exception);
					break;
				case (LoggerLevel.Debug):
					_log.Debug(toWrite, exception);
					break;
			}
		}

		protected override void WriteLine(string toWrite, LoggerLevel level)
		{
			
		}

		protected override void LogFormat(string format, object[] pars, LoggerLevel level)
		{
			var toWrite = string.Format(format, pars);
			switch (level)
			{
				case (LoggerLevel.Fatal):
					_log.Fatal(toWrite);
					break;
				case (LoggerLevel.Error):
					_log.Error(toWrite);
					break;
				case (LoggerLevel.Warning):
					_log.Warn(toWrite);
					break;
				case (LoggerLevel.Info):
					_log.Info(toWrite);
					break;
				case (LoggerLevel.Debug):
					_log.Debug(toWrite);
					break;
			}
		}
	}
}
