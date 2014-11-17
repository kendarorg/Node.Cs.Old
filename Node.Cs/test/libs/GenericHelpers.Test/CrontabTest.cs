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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericHelpers.Test
{
	[TestClass]
	public class CrontabTest
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes

		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//

		#endregion

		[TestMethod]
		public void CrontabWithDateInput()
		{
			var now = DateTime.Now;
			var target = new Crontab(now, 5000);
			var good = new DateTime(now.Ticks + 5000 * TimeSpan.TicksPerMillisecond);
			var bad = new DateTime(now.Ticks + 1040 * TimeSpan.TicksPerMillisecond);
			var next1 = target.Next();
			Thread.Sleep(100);
			var next = target.Next();
			Assert.IsTrue(target.MayRunAt(good));
			Assert.IsFalse(target.MayRunAt(bad));

			var showDiff = string.Format("Next <{0}> now <{1}>", next1, now);
			Assert.AreEqual(next1.DayOfYear, now.DayOfYear, showDiff);
			Assert.AreEqual(next1.Hour, now.Hour, showDiff);
			Assert.AreEqual(next1.Minute, now.Minute, showDiff);
			//Assert.IsTrue(Math.Abs(next1.Second - now.Second) <= 1, showDiff);

			Assert.AreEqual(next.DayOfYear, good.DayOfYear);
			Assert.AreEqual(next.Hour, good.Hour);
			Assert.AreEqual(next.Minute, good.Minute);
			Assert.IsTrue(Math.Abs(next.Second - good.Second) <= 1);

			DateTime? newNext = new DateTime(next.Ticks + 2600 * TimeSpan.TicksPerMillisecond);
			var wrongNext = target.Next(newNext);
			Assert.IsFalse(Math.Abs(newNext.Value.Second - wrongNext.Second) <= 1);
		}

		[TestMethod]
		public void FailGetNeares()
		{
			var now = DateTime.Now;
			var target = new Crontab(now, 1000);
			var good = now + TimeSpan.FromMilliseconds(1000);
			var bad = now + TimeSpan.FromMilliseconds(1002);
			Assert.IsTrue(target.MayRunAt(good));
			Assert.IsFalse(target.MayRunAt(bad));
		}

		[TestMethod]
		public void CrontabWithInvalidParameters()
		{
			Exception expex = null;
			try
			{
				var target = new Crontab(null);
				Assert.IsTrue(target == null);
			}
			catch (ArgumentNullException ex)
			{
				expex = ex;
			}
			Assert.IsTrue(expex is ArgumentNullException);

			try
			{
				var target = new Crontab("* * * * * * * *", true);
				Assert.IsTrue(target == null);
			}
			catch (IndexOutOfRangeException ex)
			{
				expex = ex;
			}
			Assert.IsTrue(expex is IndexOutOfRangeException);

			try
			{
				var target = new Crontab("@fakeCommand", true);
				Assert.IsTrue(target == null);
			}
			catch (IndexOutOfRangeException ex)
			{
				expex = ex;
			}
			Assert.IsTrue(expex is IndexOutOfRangeException);
		}


		[TestMethod]
		public void CrontabWithSeconds()
		{
			var target = new Crontab("10 */1 * * * * *", true);

			var nxt = target.Next();
			var now = DateTime.Now;
			now += TimeSpan.FromMinutes(1);
			now += TimeSpan.FromSeconds(11);
			Assert.IsTrue(now >= nxt);


			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 10)));

			target = new Crontab("* * * * * * *", true);
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));

			target = new Crontab("10* * * * * * *", true);
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 10)));

			target = new Crontab("* */1 * * * * *", true);
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003", true);
			Assert.IsTrue(target.MayRunAt(new DateTime(2003, 1, 11, 9, 5, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 11, 9, 5, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 11, 9, 6, 0)));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003-2012", true);
			Assert.IsTrue(target.MayRunAt(new DateTime(2008, 1, 11, 9, 5, 0)));

			target = new Crontab("* 0 0/1 * * * ?", true);
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 0, 0)));
		}

		/// <summary>
		///A test for Crontab Constructor
		///</summary>
		[TestMethod]
		public void CrontabMayRunAt()
		{
			var target = new Crontab("* * * * * * *");
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));

			target = new Crontab("* * * * * 9* *");
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));

			target = new Crontab("* */1 * * * * *");
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003");
			Assert.IsTrue(target.MayRunAt(new DateTime(2003, 1, 11, 9, 5, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 11, 9, 5, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 11, 9, 6, 0)));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003-2012");
			Assert.IsTrue(target.MayRunAt(new DateTime(2008, 1, 11, 9, 5, 0)));

			target = new Crontab("* 0 0/1 * * * ?");
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 1, 1, 0)));
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 1, 0, 0)));
		}

		/// <summary>
		///A test for Crontab Constructor
		///</summary>
		[TestMethod]
		public void CrontabNextRun()
		{
			var target = new Crontab("* * * * * * *");
			Assert.AreEqual(target.Next(new DateTime(2000, 1, 1, 1, 1, 0, 0)), new DateTime(2000, 1, 1, 1, 1, 0, 0));

			Assert.AreEqual(target.Next(new DateTime(2200, 1, 1, 1, 1, 0)), new DateTime(2200, 1, 1, 1, 1, 0));

			Assert.AreEqual(target.Next(new DateTime(1000, 1, 1, 1, 1, 0)), new DateTime(1000, 1, 1, 1, 1, 0));

			target = new Crontab("* */1 * * * * *");
			Assert.AreEqual(target.Next(new DateTime(2000, 1, 1, 1, 1, 0)), new DateTime(2000, 1, 1, 1, 1, 0));

			target = new Crontab("* * * * * * 2008");
			Assert.AreEqual(target.Next(new DateTime(2007, 12, 31, 23, 59, 59)), new DateTime(2008, 1, 1, 0, 0, 0));

			target = new Crontab("* * * * 2 * 2008");
			Assert.AreEqual(target.Next(new DateTime(2008, 1, 31, 23, 59, 59)), new DateTime(2008, 2, 1, 0, 0, 0));

			target = new Crontab("* * * 2 * * 2008");
			Assert.AreEqual(target.Next(new DateTime(2008, 1, 1, 23, 59, 59)), new DateTime(2008, 1, 2, 0, 0, 0));

			target = new Crontab("* * * * * 3 *");
			Assert.AreEqual(target.Next(new DateTime(2012, 11, 27, 23, 59, 59)), new DateTime(2012, 11, 28, 0, 0, 0));

			target = new Crontab("* * * * * 1 *");
			Assert.AreEqual(target.Next(new DateTime(2012, 11, 27, 23, 59, 59)), new DateTime(2012, 12, 3, 0, 0, 0));

			target = new Crontab("* * * * * 1 *");
			Assert.AreEqual(target.Next(new DateTime(2012, 11, 27, 23, 59, 59)), new DateTime(2012, 12, 3, 0, 0, 0));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003");
			Assert.AreEqual(target.Next(new DateTime(2003, 1, 11, 9, 5, 0)), new DateTime(2003, 1, 11, 9, 5, 0));
			Assert.AreEqual(target.Next(new DateTime(2003, 1, 11, 9, 6, 0)), new DateTime(2003, 1, 11, 9, 8, 0));
			Assert.AreEqual(target.Next(new DateTime(2003, 1, 11, 23, 6, 0)), new DateTime(2003, 1, 12, 1, 2, 0));
			Assert.AreEqual(target.Next(new DateTime(2003, 1, 26, 23, 00, 0)), new DateTime(2003, 2, 11, 1, 2, 0));

			target = new Crontab("* 2-59/3 1,9,22 11-26 1-6 ? 2003-2012");
			Assert.AreEqual(target.Next(new DateTime(2008, 7, 11, 9, 5, 0)), new DateTime(2009, 1, 11, 1, 2, 0));
		}

		/// <summary>
		///A test for Crontab Constructor
		///</summary>
		[TestMethod]
		public void CrontabSpecial()
		{
			var target = new Crontab("@yearly");
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 0, 0, 0)));
			Assert.IsTrue(target.MayRunAt(new DateTime(2001, 1, 1, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 0, 1, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 1, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 2, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 2, 1, 0, 0, 0)));

			target = new Crontab("@annually");
			Assert.IsTrue(target.MayRunAt(new DateTime(2000, 1, 1, 0, 0, 0)));
			Assert.IsTrue(target.MayRunAt(new DateTime(2001, 1, 1, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 0, 1, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 1, 1, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 1, 2, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 2, 1, 0, 0, 0)));

			target = new Crontab("@monthly");
			Assert.IsTrue(target.MayRunAt(new DateTime(2001, 2, 1, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2000, 2, 1, 0, 1, 0)));


			target = new Crontab("@weekly");
			Assert.IsTrue(target.MayRunAt(new DateTime(2012, 4, 15, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2012, 4, 18, 0, 0, 0)));

			target = new Crontab("@daily");
			Assert.IsTrue(target.MayRunAt(new DateTime(2012, 4, 15, 0, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2012, 4, 15, 0, 1, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2012, 4, 18, 1, 0, 0)));

			target = new Crontab("@hourly");
			Assert.IsTrue(target.MayRunAt(new DateTime(2012, 4, 15, 1, 0, 0)));
			Assert.IsFalse(target.MayRunAt(new DateTime(2012, 4, 15, 1, 1, 0)));
		}

		[TestMethod]
		public void CrontabExceed()
		{
			var dt = new DateTime(2000, 1, 1, 1, 0, 0);
			var target = new Crontab("* */80 * * * * *");
			Assert.IsTrue(target.MayRunAt(dt));
			dt += TimeSpan.FromMinutes(40);
			Assert.IsFalse(target.MayRunAt(dt));
			dt += TimeSpan.FromMinutes(40);
			Assert.IsFalse(target.MayRunAt(dt));
		}

		[TestMethod]
		public void TestGetNextNearestItem()
		{
			var privateObject = new PrivateObject(new Crontab("* */80 * * * * *"));
			//Invoking the private method
			var dateRange = privateObject.Invoke("CreateDateRange");
			var result = privateObject.Invoke("GetNextNearestItem", 10, dateRange, 1) as Int32?;
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result);
		}
	}
}
