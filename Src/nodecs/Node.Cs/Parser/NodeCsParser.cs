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
using System.Globalization;
using System.Linq;
using CoroutinesLib.Shared;
using GenericHelpers;
using NodeCs.Shared;

namespace NodeCs.Parser
{
	internal class NodeCsParser
	{


		private readonly CommandLineParser _clp;
		private readonly string _configPath;
		private  Exception _lastException;

		public NodeCsParser(CommandLineParser clp, string configPath, ICoroutinesManager coroutinesManager)
		{
			_clp = clp;
			_configPath = configPath;
		}

		public bool Execute(string result)
		{
			var tokens = Tokenize(result);
			var first = tokens.FirstOrDefault();
			if (first == null)
			{
				Shared.NodeRoot.Write();
			}
			else if (first.Type == TokenType.Command && first.Value == "exit")
			{
				return false;
			}
			else if (first.Type == TokenType.Command && first.Value == "lasterror")
			{
				if (_lastException == null)
				{
					Shared.NodeRoot.CWriteLine("No previous errors.");
				}
				else
				{
					Shared.NodeRoot.CWriteLine(_lastException.Message);
					Shared.NodeRoot.CWriteLine(_lastException.ToString());
				}
				return true;
			}
			else if (first.Type == TokenType.Command)
			{
				var functionIndex = (first.Value + (tokens.Count - 1)).ToLowerInvariant();
				if (!Commands.Functions.ContainsKey(functionIndex))
				{
					Shared.NodeRoot.CWriteLine(string.Format("Command not found. Type 'help {0}' to get the parameters.", first.Value));
					return true;
				}
				var cmd = Commands.Functions[functionIndex];
				if (tokens.Count != cmd.ParametersCount + 1)
				{
					Shared.NodeRoot.CWriteLine(string.Format("Command '{0}' requires '{1}' parameters.", first.Value, cmd.ParametersCount));
				}
				else
				{
					try
					{
						ExecuteCommand(cmd, tokens);
					}
					catch (Exception ex)
					{
						Shared.NodeRoot.CWriteLine(string.Format("Error executing: {0}", result));
						Shared.NodeRoot.CWriteLine(ex.ToString());
					}
				}
			}
			else
			{
				Shared.NodeRoot.CWriteLine(string.Format("Invalid command: {0}", result));
			}
			return true;
		}

		private List<NodeCsToken> Tokenize(string result)
		{
			var tokens = new List<NodeCsToken>();
			var tmp = string.Empty;
			for (int index = 0; index < result.Length; index++)
			{
				var c = result[index];
				if (IsStringStart(c))
				{
					tmp = string.Empty;
					index++;
					var end = FindEndOfString(c, index, result);
					tokens.Add(new NodeCsToken(result.Substring(index, end - index), TokenType.String));
					index = end + 1;
				}
				else if (IsSeparator(c))
				{
					if (tmp.Length > 0)
					{
						tokens.Add(BuildCommand(tmp));
					}
					tmp = string.Empty;
				}
				else
				{
					tmp += c;
				}
			}
			if (tmp.Length > 0)
			{
				tokens.Add(BuildCommand(tmp));
			}
			return tokens;
		}

		private NodeCsToken BuildCommand(string tokenString)
		{
			tokenString = tokenString.ToLowerInvariant().Trim();
			if (Commands.Functions.Any(c => c.Key.StartsWith(tokenString)))
			{
				return new NodeCsToken(tokenString, TokenType.Command);
			}
			double result;
			if (double.TryParse(tokenString, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return new NodeCsToken(tokenString, TokenType.Numeric);
			}
			return new NodeCsToken(tokenString, TokenType.String);
		}

		private bool IsSeparator(char c)
		{
			return c == ' ' || c == '\t';
		}

		private int FindEndOfString(char startChar, int index, string result)
		{
			char prevChar = startChar;
			for (; index < result.Length; index++)
			{
				var current = result[index];
				if (IsStringEnd(startChar, prevChar, current))
				{
					return index;
				}
			}
			throw new Exception("Missing end of string");
		}
		private bool IsStringEnd(char start, char prev, char current)
		{
			return current == start && prev != '\\';
		}
		private bool IsStringStart(char c)
		{
			return c == '\"' || c == '\'';
		}

		private void ExecuteCommand(CommandDefinition cmd, List<NodeCsToken> tokens)
		{
			try
			{
				var type = cmd.Action.GetType();
				if (type == typeof(Action))
				{
					((Action)cmd.Action)();
				}
				else if (type == typeof(Action<string>))
				{
					((Action<string>)cmd.Action)(tokens[1].Value);
				}
				else if (type == typeof(Action<string, string>))
				{
					((Action<string, string>)cmd.Action)(tokens[1].Value, tokens[2].Value);
				}
				else if (type == typeof(Action<string, string,string>))
				{
					((Action<string, string, string>)cmd.Action)(tokens[1].Value, tokens[2].Value, tokens[3].Value);
				}
				else
				{
					throw new Exception("Unsupported command");
				}
			}
			catch (Exception ex)
			{
				_lastException = ex;
				Shared.NodeRoot.CWriteLine("Error executing command:");
				Shared.NodeRoot.CWriteLine(ex.Message);
			}
		}
	}
}
