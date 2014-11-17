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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericHelpers.Test
{
	[TestClass]
	public class ReflectionUtilsTest
	{
		[TestMethod]
		public void ReflectionUtils_MustBeUsable()
		{
		}
	}
	[TestClass]
	public class CommandLineParserTest
	{
		[TestMethod]
		public void ItShouldBePossibleToCreateACommandLneParser()
		{
			ShowHelpCalled = 0;
			const string helpString = "help";
			var args = new[] { "-test", "-parameter", "parameterValue" };
			var commandLineParser = new CommandLineParser(args, helpString);
			var resultValue = commandLineParser["test"];
			Assert.IsTrue(commandLineParser.IsSet("test"));
			Assert.IsTrue(resultValue.Length == 0);
			resultValue = commandLineParser["notPresent"];
			Assert.IsNull(resultValue);
			Assert.IsFalse(commandLineParser.IsSet("notPresent"));
			resultValue = commandLineParser["parameter"];
			Assert.IsTrue(commandLineParser.IsSet("parameter"));
			Assert.AreEqual("parameterValue", resultValue);

			commandLineParser["parameter"] = "newValue";
			Assert.IsTrue(commandLineParser.IsSet("parameter"));
			resultValue = commandLineParser["parameter"];
			Assert.AreEqual("newValue", resultValue);
		}

		[TestMethod]
		public void ItShouldBePossibleToCreateACommandLneParserAndShowHelp()
		{
			ShowHelpCalled = 0;
			const string helpString = "help";
			var args = new[] { "-test", "-help" };
			var commandLineParser = new CommandLineParser(args, helpString, () => { ShowHelpCalled++; });
			Assert.AreEqual(helpString, commandLineParser.Help);
			Assert.IsTrue(commandLineParser.IsSet("help"));
			Assert.AreEqual(1, ShowHelpCalled);
		}

		[TestMethod]
		public void ItShouldBePossibleToCheckForValuesPresence()
		{
			ShowHelpCalled = 0;
			const string helpString = "help";
			var args = new[] { "-test", "-gino", "-pino", "pinoValue" };
			var commandLineParser = new CommandLineParser(args, helpString, () => { ShowHelpCalled++; });

			Assert.IsTrue(commandLineParser.Has(new[] { "test", "gino" }));
			Assert.IsFalse(commandLineParser.Has(new[] { "test", "fake" }));
			Assert.IsFalse(commandLineParser.Has(new[] { "fluke", "fake" }));

			Assert.IsTrue(commandLineParser.HasAllOrNone(new[] { "test", "gino" }));
			Assert.IsFalse(commandLineParser.HasAllOrNone(new[] { "test", "fake" }));
			Assert.IsTrue(commandLineParser.HasAllOrNone(new[] { "fluke", "fake" }));

			Assert.IsFalse(commandLineParser.HasOneAndOnlyOne(new[] { "test", "gino" }));
			Assert.IsTrue(commandLineParser.HasOneAndOnlyOne(new[] { "test", "fake" }));
			Assert.IsFalse(commandLineParser.HasOneAndOnlyOne(new[] { "fluke", "fake" }));

			Assert.AreEqual(0, ShowHelpCalled);
		}


		[TestMethod]
		public void ItShouldBePossibleToGetEnvironmentVariables()
		{
			ShowHelpCalled = 0;
			var args = new[] { "-test" };
			var commandLineParser = new CommandLineParser(args, "help");
			var temp = CommandLineParser.GetEnv("TEMP");
			commandLineParser["TEMP"] = temp;
			Assert.IsTrue(Directory.Exists(temp));
			var os = CommandLineParser.GetEnv("OS");
			commandLineParser["os"] = os;

			var notExistingVariable = CommandLineParser.GetEnv("thisDoesNotExists" + Guid.NewGuid());
			Assert.IsNull(notExistingVariable);

			Assert.IsTrue(commandLineParser.IsSet("os"));
			Assert.IsTrue(commandLineParser.IsSet("oS"));
			Assert.AreEqual(commandLineParser["os"], commandLineParser["oS"]);

			Assert.IsTrue(commandLineParser.IsSet("Temp"));
			Assert.IsTrue(commandLineParser.IsSet("temP"));
			Assert.AreEqual(commandLineParser["TEMP"], commandLineParser["temp"]);

			Assert.IsFalse(string.IsNullOrWhiteSpace(os));
		}

		public int ShowHelpCalled = 0;


		[Ignore]
		[TestMethod]
		public void ShouldBeAbleToHandleMultipleItems()
		{

		}
		[Ignore]
		[TestMethod]
		public void ShouldBeAbleToSetGetEnvironmentVariables()
		{

		}
	}
}
