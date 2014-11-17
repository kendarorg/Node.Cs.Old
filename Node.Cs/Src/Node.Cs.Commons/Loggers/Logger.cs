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
using System.Collections.ObjectModel;

namespace Node.Cs.Lib.Loggers
{
	public static class Logger
	{
		// ReSharper disable InconsistentNaming
		public const string _ERROR = "ERROR";
		public const string _WARNING = "WARN ";
		public static string _INFO ="INFO ";
		// ReSharper restore InconsistentNaming

		private readonly static List<ILogger> _loggers = new List<ILogger>();
		private static ReadOnlyCollection<ILogger> _real = new ReadOnlyCollection<ILogger>(new List<ILogger>());
		
		public static void RegisterLogger(ILogger logger)
		{
			_loggers.Add(logger);
		}

		public static void Error(Exception ex, string message = null, params object[] pars)
		{
			foreach (var logger in _real)
			{
				if (logger.IsErrorEnabled) logger.Error(ex, message, pars);
			}
		}

		public static void Error(string message, params object[] pars)
		{
			foreach (var logger in _real)
			{
				if (logger.IsErrorEnabled) logger.Error(message, pars);
			}
		}

		public static void Warn(string message, params object[] pars)
		{
			foreach (var logger in _real)
			{
				if (logger.IsWarningEnabled) logger.Warn(message, pars);
			}
		}

		public static void Submit()
		{
			_real = new ReadOnlyCollection<ILogger>(_loggers);
		}

		public static void Info(string message, params object[] pars)
		{
			foreach (var logger in _real)
			{
				if (logger.IsInfoEnabled) logger.Info(message, pars);
			}
		}
	}
}
