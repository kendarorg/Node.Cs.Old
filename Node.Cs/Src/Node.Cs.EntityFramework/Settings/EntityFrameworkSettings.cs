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


using Node.Cs.Lib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Node.Cs.EntityFramework.Settings
{
	public class EntityFrameworkSettings : INodeCsSettingsRoot
	{
		public DefaultConnectionFactoryDefinition DefaultConnectionFactory { get; set; }
		[XmlArrayItem("Provider", typeof(ProviderDefinition))]
		[XmlArray("Providers")]
		public List<ProviderDefinition> Providers { get; set; }

		public string SettingsTag
		{
			get { return "EntityFramework"; }
		}
	}

	public class ProviderDefinition
	{
		[XmlAttribute("InvariantName")]
		public string InvariantName { get; set; }

		[XmlAttribute("Type")]
		public string ProviderType { get; set; }
	}

	public class DefaultConnectionFactoryDefinition
	{
		public DefaultConnectionFactoryDefinition()
		{
			Parameters = new List<EfParameter>();
		}

		[XmlAttribute("Type")]
		public string FactoryType { get; set; }

		[XmlArrayItem("Parameter", typeof(EfParameter))]
		[XmlArray("Parameters")]
		public List<EfParameter> Parameters { get; set; }
	}

	public class EfParameter
	{
		[XmlAttribute("Value")]
		public string Value { get; set; }

		public override string ToString()
		{
			return Value;
		}
	}
}
