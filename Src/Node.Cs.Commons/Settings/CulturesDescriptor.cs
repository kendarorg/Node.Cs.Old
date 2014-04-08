using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Node.Cs.Lib.Settings
{
	public class CulturesDefinition
	{
		private string _defaultCultureString;
		private string _availableCultureStrings;
		private ConcurrentDictionary<string, CultureInfo> _availableCultures = 
			new ConcurrentDictionary<string,CultureInfo>(
				new Dictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase),StringComparer.OrdinalIgnoreCase);
		private CultureInfo _defaultCulture = new CultureInfo("en-US");

		[XmlAttribute("Default")]
		public string DefaultCultureString
		{
			get { return _defaultCultureString; }
			set
			{
				_defaultCultureString = value;
				if (!string.IsNullOrWhiteSpace(_defaultCultureString))
				{
					_defaultCulture = new CultureInfo(_defaultCultureString);
				}
			}
		}

		[XmlAttribute("Available")]
		public string AvailableCultureStrings
		{
			get { return _availableCultureStrings; }
			set
			{
				if (value == null) value = string.Empty;
				_availableCultureStrings = value;
				var allCultures = _availableCultureStrings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				var tmpList = new List<CultureInfo>();
				foreach (var culture in allCultures)
				{
					_availableCultures.TryAdd(culture,new CultureInfo(culture));
				}
			}
		}

		[XmlIgnore]
		public IDictionary<string,CultureInfo> AvailableCultures
		{
			get
			{
				return _availableCultures;
			}
		}

		[XmlIgnore]
		public CultureInfo DefaultCulture
		{
			get
			{
				return _defaultCulture;
			}
		}
	}
}
