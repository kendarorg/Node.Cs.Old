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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace Node.Cs.Lib.Settings
{
	public class CulturesDefinition
	{
		private string _defaultCultureString;
		private string _availableCultureStrings;
		private readonly ConcurrentDictionary<string, CultureInfo> _availableCultures = 
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
