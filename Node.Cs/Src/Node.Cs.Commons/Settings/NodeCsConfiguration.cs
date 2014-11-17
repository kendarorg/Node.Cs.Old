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
using System.Xml;
using System.Xml.Serialization;
using Node.Cs.Lib.Exceptions;

namespace Node.Cs.Lib.Settings
{

	public class NodeCsConfiguration
	{
		private const string XML_PREAMBLE = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>";
		private class NodeSection
		{
			public string Content;
			public object ParsedData;
		}

		private static Dictionary<string, NodeSection> _sections;

		public static void InitializeConfigurations(string nodeCsConfig, NodeCsSettings defaultSettings)
		{
			_sections = new Dictionary<string, NodeSection>(StringComparer.OrdinalIgnoreCase);
			var doc = new XmlDocument();
			doc.Load(nodeCsConfig);
			foreach (XmlNode sectionNode in doc.DocumentElement.ChildNodes)
			{
				var childNodeName = sectionNode.Name.ToLowerInvariant();
				var childContent = string.Format("{0}<{1}>{2}</{1}>", XML_PREAMBLE, sectionNode.Name, sectionNode.InnerXml);
				_sections.Add(childNodeName, new NodeSection
				{
					Content = childContent
				});
			}
			if (!_sections.ContainsKey("NodeCsSettings"))
			{
				_sections.Add("NodeCsSettings", new NodeSection
				{
					Content = string.Empty,
					ParsedData = defaultSettings
				});
			}
			var nodeCsSettings = GetSection<NodeCsSettings>("NodeCsSettings");
			if (nodeCsSettings == null)
			{
				throw new NodeCsException("Missing node.cs settings section in node.config.");
			}
		}

		public static T GetSection<T>(string sectionName)
		{
			if (!_sections.ContainsKey(sectionName))
			{
				throw new NodeCsException("Missing '{0}' section in node.config.", sectionName);
			}
			var section = _sections[sectionName];
			if (section.ParsedData == null)
			{
				var serializer = new XmlSerializer(typeof(T));
				var reader = new StringReader(section.Content);
				var parsedData = (T)serializer.Deserialize(reader);
				section.ParsedData = parsedData;
			}
			return (T)section.ParsedData;
		}

	}
}
