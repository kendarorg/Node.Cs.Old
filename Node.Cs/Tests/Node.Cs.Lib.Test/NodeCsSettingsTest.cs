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


using System.IO;
using System.Xml.Serialization;
using GenericHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Cs.Lib.Settings;

namespace Node.Cs.Lib.Test
{
	[TestClass]
	public class NodeCsSettingsTest
	{
		[TestMethod]
		public void SettingsShouldBeSerializable()
		{
			var settings = NodeCsSettings.Defaults(@"C:\temp");
			var serializer = new XmlSerializer(settings.GetType());
			var writer = new StreamWriter("settings.xml");
			serializer.Serialize(writer.BaseStream, settings);
		}


		[TestMethod]
		public void SettingsShouldBeDeserializable()
		{
			var settings = new NodeCsSettings();

			var serializer = new XmlSerializer(settings.GetType());

			var settingsXml = ResourceContentLoader.LoadText("settings.xml");
			var reader = new StringReader(settingsXml);
			// ReSharper disable once UnusedVariable
			var d = (NodeCsSettings)serializer.Deserialize(reader);
			Assert.IsNotNull(d.Factories);
			Assert.IsNotNull(d.Factories.ControllersFactory);

		}
	}
}
