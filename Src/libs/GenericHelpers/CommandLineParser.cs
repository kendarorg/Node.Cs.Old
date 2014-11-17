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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace GenericHelpers
{
	public class CommandLineParser
	{
		private static Dictionary<string, string> _kvps;

		private static readonly object _lockObject = new object();
		private readonly Dictionary<string, List<string>> _commandLineValues;
		private readonly Action _exitBehaviour;
		private readonly char _separator;
		private readonly string _helpMessage;

		public CommandLineParser(string[] args, string helpMessage, Action exitBehaviour = null, char separator = ';', params string[] allowMultiple)
		{
			InitializeEnvironmentVariables();

			_helpMessage = helpMessage;
			_exitBehaviour = exitBehaviour;
			_separator = separator;
			var allowMultiple1 = new HashSet<string>(allowMultiple, StringComparer.InvariantCultureIgnoreCase);
			_commandLineValues = new Dictionary<string, List<string>>();
			for (int index = 0; index < args.Length; index++)
			{
				var item = args[index];
				if (item.StartsWith("-"))
				{
					var itemIndex = item.Substring(1).ToLowerInvariant();
					if (index < (args.Length - 1))
					{
						var nextItem = args[index + 1];
						if (!nextItem.StartsWith("-"))
						{
							if (!_commandLineValues.ContainsKey(itemIndex))
							{
								_commandLineValues[itemIndex] = new List<string>();
							}
							else if (!allowMultiple1.Contains(itemIndex))
							{
								throw new Exception(string.Format("Multiple key not allowed for '{0}'", item.Substring(1)));
							}
							_commandLineValues[itemIndex].Add(nextItem);
							continue;
						}
					}
					_commandLineValues.Add(itemIndex, new List<string> { string.Empty });
				}
			}
			if (IsSet("help") || IsSet("h"))
			{
				ShowHelp();
			}
		}

		public string Help
		{
			get { return _helpMessage; }
		}

		public bool IsMultiple(string index)
		{
			index = index.ToLowerInvariant();
			return _commandLineValues[index].Count > 1;
		}

		public IEnumerable<string> GetMultiple(string index)
		{
			index = index.ToLowerInvariant();
			if (!IsSet(index)) yield break;
			foreach (var item in _commandLineValues[index])
			{
				yield return item;
			}
		}

		public string this[string index]
		{
			get
			{
				index = index.ToLowerInvariant();
				if (IsSet(index))
				{
					if (!IsMultiple(index))
					{
						return string.Join(_separator.ToString(CultureInfo.InvariantCulture), _commandLineValues[index]);
					}
					return _commandLineValues[index][0];
				}
				return null;
			}
			set
			{
				index = index.ToLowerInvariant();
				if (!_commandLineValues.ContainsKey(index))
				{
					_commandLineValues.Add(index, new List<string>());
				}
				if (string.IsNullOrWhiteSpace(value))
				{
					_commandLineValues[index].Add(string.Empty);
					return;
				}
				_commandLineValues[index].Clear();
				var val = value.Split(new[] { _separator });
				foreach (var commandLineValue in val)
				{
					_commandLineValues[index].Add(commandLineValue);
				}
			}
		}

		private static void LoadEnvironmentVariables(EnvironmentVariableTarget target, bool none = false)
		{
			IDictionary environmentVariables = none ? Environment.GetEnvironmentVariables() : Environment.GetEnvironmentVariables(target);

			foreach (DictionaryEntry de in environmentVariables)
			{
				var lowerKey = ((string)de.Key).ToLowerInvariant();
				if (!_kvps.ContainsKey(lowerKey))
				{
					_kvps.Add(lowerKey, (string)de.Value);
				}
			}
		}

		public static string GetEnv(string envVar)
		{
			envVar = envVar.ToLowerInvariant();
			InitializeEnvironmentVariables();
			if (_kvps.ContainsKey(envVar))
			{
				return _kvps[envVar];
			}
			return null;
		}

		public static void SetEnv(string envVar, string val)
		{
			envVar = envVar.ToLowerInvariant();
			InitializeEnvironmentVariables();
			if (_kvps.ContainsKey(envVar))
			{
				_kvps[envVar] = val;
			}
			else
			{
				_kvps.Add(envVar, val);
			}
		}

		private static void InitializeEnvironmentVariables()
		{
			if (_kvps == null)
			{
				lock (_lockObject)
				{
					_kvps = new Dictionary<string, string>();
					LoadEnvironmentVariables(EnvironmentVariableTarget.Process, true);
					LoadEnvironmentVariables(EnvironmentVariableTarget.Process);
					LoadEnvironmentVariables(EnvironmentVariableTarget.User);
					LoadEnvironmentVariables(EnvironmentVariableTarget.Machine);
				}
			}
		}

		public string GetOrDefault(string index, string defaultValue)
		{
			if (Has(index)) return this[index];
			return defaultValue;
		}

		public bool IsSet(string index)
		{
			index = index.ToLowerInvariant();
			return _commandLineValues.ContainsKey(index) && _commandLineValues[index].Count > 0;
		}

		public bool Has(params string[] vals)
		{
			foreach (var item in vals)
			{
				var index = item.ToLowerInvariant();
				if (!IsSet(index)) return false;
			}
			return true;
		}

		public bool HasAllOrNone(params string[] vals)
		{
			int setted = 0;
			foreach (var item in vals)
			{
				var index = item.ToLowerInvariant();
				if (IsSet(index)) setted++;
			}
			if (setted == 0 || setted == vals.Length) return true;
			return false;
		}

		public bool HasOneAndOnlyOne(params string[] vals)
		{
			bool setted = false;
			foreach (var item in vals)
			{
				var index = item.ToLowerInvariant();
				if (IsSet(index))
				{
					if (setted)
					{
						return false;
					}
					setted = true;
				}
			}
			return setted;
		}

		public void ShowHelp()
		{
			Console.WriteLine(_helpMessage);
			if (_exitBehaviour != null) _exitBehaviour();
		}
	}
}
