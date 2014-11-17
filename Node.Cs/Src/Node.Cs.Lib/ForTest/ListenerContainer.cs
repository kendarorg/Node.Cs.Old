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



using NetworkHelpers.Http;

namespace Node.Cs.Lib.ForTest
{
	public interface IListenerContainer
	{
		bool HasConnection { get; }
		bool HasContext { get; }
		bool HasRequest { get; }
		bool HasUserLanguage { get; }
		string[] UserLanguages { get; }
		object Context { get; }
	}

	public class ListenerContainer : IListenerContainer
	{
		private readonly ConnectionHttp _connection;

		public ListenerContainer(ConnectionHttp connection)
		{
			_connection = connection;
		}

		public bool HasConnection { get { return _connection != null; } }
		public bool HasContext { get { return HasConnection && _connection.Context != null; } }
		public bool HasRequest { get { return HasContext && _connection.Context.Request != null; } }

		public bool HasUserLanguage
		{
			get
			{
				return HasRequest && _connection.Context.Request.UserLanguages != null;
			}
		}

		public string[] UserLanguages
		{
			get
			{
				if (!HasUserLanguage) return new string[] { };
				return _connection.Context.Request.UserLanguages;
			}
		}


		public object Context
		{
			get
			{
				{
					if (!HasContext) return null;
					return _connection.Context;
				}
			}
		}
	}
}
