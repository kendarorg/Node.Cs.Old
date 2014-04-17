
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
