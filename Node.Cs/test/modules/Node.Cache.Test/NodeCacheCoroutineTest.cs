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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoroutinesLib;
using CoroutinesLib.Shared;
using CoroutinesLib.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Caching;
using NodeCs.Shared.Caching;

namespace Node.Cache.Test
{
	[TestClass]
	public class NodeCacheTest
	{
		[ClassInitialize]
		public static void SetUp(TestContext ctx)
		{
			RunnerFactory.Initialize();
			_runner = new RunnerForTest();
		}

		// ReSharper disable ReturnValueOfPureMethodIsNotUsed
		[TestMethod]
		public void ShouldExistTheDefaultEmptyGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			Assert.AreEqual(1, target.Groups.Count);
			var group = target.Groups[string.Empty];
			Assert.IsNotNull(group);
		}


		#region TimeBased tests

		[TestMethod]
		public void WhenTimeoutIsReachedNoDataIsReturned()
		{
			_loadDataCalled = 0;
			object result = null;
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			var waiter = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadDataWithWait(20, 5),
				ExpireAfter = TimeSpan.FromMilliseconds(3)
			}, (r) => result = r);
			target.Execute().Count();	//Initialize 
			var task = Task.Run(() =>
			{
				for (int i = 0; i < 8; i++)
				{
					_runner.RunCycle();//Fun the getter function
				}
			});
			Task.WaitAll(task);
			target.Execute().Count();	//Initialize 
			target.Execute().Count();	//Initialize 


			Assert.IsNull(result);
			Assert.AreEqual(1, _loadDataCalled);
		}

		[TestMethod]
		public void ItShouldBePossibleToAddAndGetNotExistingItem()
		{
			_loadDataCalled = 0;
			object result = null;
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			var cr = new NodeCacheCoroutine(target);
			_runner.StartCoroutine(cr);
			_runner.RunCycle();

			var waiter = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadDataWithWait(10, 5)
			}, (r) =>
			{
				result = r;
			});
			_runner.StartCoroutine(waiter.AsCoroutine());
			_runner.RunCycleFor(100);

			Assert.AreEqual(1, _loadDataCalled);
			Assert.AreEqual("RESULT", result);
		}
		#endregion

		#region Groups Management
		[TestMethod]
		public void ItShouldBePossibleToAddGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			target.AddGroup(new CacheGroupDefinition
				{
					Id = "test",
					Capped = 1000,
					ExpireAfter = TimeSpan.FromMilliseconds(500),
					RollingExpiration = true
				});
			target.Execute().Count();

			Assert.AreEqual(2, target.Groups.Count);
			Assert.IsTrue(target.Groups.ContainsKey("test"));
		}

		[TestMethod]
		public void ItShouldBePossibleToAddGroupUpdatingShouldThrow()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddGroup(new CacheGroupDefinition
				{
					Id = "test",
					Capped = 1000,
					ExpireAfter = TimeSpan.FromMilliseconds(500),
					RollingExpiration = true
				});
			target.Execute().Count();

			Exception resultEx = null;
			try
			{
				target.AddGroup(new CacheGroupDefinition
				{
					Id = "test",
					Capped = 100,
					ExpireAfter = TimeSpan.FromMilliseconds(500),
					RollingExpiration = true
				});
			}
			catch (Exception ex)
			{
				resultEx = ex;
			}
			Assert.IsNotNull(resultEx);
		}

		[TestMethod]
		public void ItShouldBePossibleToRemoveGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddGroup(new CacheGroupDefinition
			{
				Id = "test",
				Capped = 1000,
				ExpireAfter = TimeSpan.FromMilliseconds(500),
				RollingExpiration = true
			});
			target.Execute().Count();
			Assert.AreEqual(2, target.Groups.Count);

			target.RemoveGroup("test");
			target.Execute().Count();

			Assert.AreEqual(1, target.Groups.Count);
			Assert.IsFalse(target.Groups.ContainsKey("test"));
		}

		[TestMethod]
		public void ItShouldBePossibleToRemoveGroupNoGroupShouldDoNothing()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			target.RemoveGroup("notExisting");
			target.Execute().Count();

			Assert.AreEqual(1, target.Groups.Count);
			Assert.IsFalse(target.Groups.ContainsKey("notExisting"));
		}
		#endregion

		#region AddItem
		[TestMethod]
		public void ItShouldBePossibleToAddItemToSpecificGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddGroup(new CacheGroupDefinition
			{
				Id = "testGroup",
				Capped = 1000,
				ExpireAfter = TimeSpan.FromMilliseconds(500),
				RollingExpiration = true
			});
			target.Execute().Count();
			target.AddItem("testId", "testValue", "testGroup");
			target.Execute().Count();

			var group = target.Groups["testGroup"];
			Assert.IsTrue(target.GetItems("testGroup").ContainsKey("testId"));
			var item = target.GetItems("testGroup")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
		}

		[TestMethod]
		public void ItShouldBePossibleToAddItemToDefaultGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem("testId", "testValue");
			target.Execute().Count();

			var group = target.Groups[string.Empty];
			Assert.IsTrue(target.GetItems("").ContainsKey("testId"));
			var item = target.GetItems("")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
		}

		[TestMethod]
		public void ItShouldBePossibleToAddItemDuplicateShouldDoNothing()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem("testId", "testValue");
			target.Execute().Count();


			target.AddItem("testId", "testValueChange");
			target.Execute().Count();

			var item = target.GetItems("")["testId"];
			Assert.AreEqual("testValue", item.Value);
		}


		[TestMethod]
		public void ItShouldBePossibleToAddItemWithCd()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem(new CacheDefinition
				{
					Value = "testValue",
					Id = "testId"
				});
			target.Execute().Count();

			var group = target.Groups[string.Empty];
			Assert.IsTrue(target.GetItems("").ContainsKey("testId"));
			var item = target.GetItems("")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
			Assert.IsFalse(target.Groups.ContainsKey("test"));
		}

		[TestMethod]
		public void ItShouldBePossibleToAddItemDuplicateShouldDoNothingWithCd()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem(new CacheDefinition
			{
				Value = "testValue",
				Id = "testId"
			});
			target.Execute().Count();


			target.AddItem(new CacheDefinition
			{
				Value = "testValueDifferent",
				Id = "testId"
			});
			target.Execute().Count();

			var item = target.GetItems("")["testId"];
			Assert.AreEqual("testValue", item.Value);
		}

		#endregion

		#region AddOrUpdateItem

		[TestMethod]
		public void ItShouldBePossibleToAddUpdateItemToSpecificGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddGroup(new CacheGroupDefinition
			{
				Id = "testGroup",
				Capped = 1000,
				ExpireAfter = TimeSpan.FromMilliseconds(500),
				RollingExpiration = true
			});
			target.Execute().Count();
			target.AddOrUpdateItem("testId", "testValue", "testGroup");
			target.Execute().Count();

			var group = target.Groups["testGroup"];
			Assert.IsTrue(target.GetItems("testGroup").ContainsKey("testId"));
			var item = target.GetItems("testGroup")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
		}

		[TestMethod]
		public void ItShouldBePossibleToAddUpdateItemToDefaultGroup()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddOrUpdateItem("testId", "testValue");
			target.Execute().Count();

			var group = target.Groups[string.Empty];
			Assert.IsTrue(target.GetItems("").ContainsKey("testId"));
			var item = target.GetItems("")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
		}

		[TestMethod]
		public void ItShouldBePossibleToAddUpdateItemDuplicateShouldUpdate()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem("testId", "testValue");
			target.Execute().Count();

			target.AddOrUpdateItem("testId", "testValueChange");
			target.Execute().Count();

			var item = target.GetItems("")["testId"];
			Assert.AreEqual("testValueChange", item.Value);
		}


		[TestMethod]
		public void ItShouldBePossibleToAddUpdateItemWithCd()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddOrUpdateItem(new CacheDefinition
			{
				Value = "testValue",
				Id = "testId"
			});
			target.Execute().Count();

			var group = target.Groups[string.Empty];
			Assert.IsTrue(target.GetItems("").ContainsKey("testId"));
			var item = target.GetItems("")["testId"];
			Assert.AreEqual(group.ExpireAfter, item.ExpireAfter);
			Assert.AreEqual("testValue", item.Value);
			Assert.IsFalse(target.Groups.ContainsKey("test"));
		}

		[TestMethod]
		public void ItShouldBePossibleToAddUpdateItemDuplicateShouldUpdateCd()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem(new CacheDefinition
			{
				Value = "testValue",
				Id = "testId"
			});
			target.Execute().Count();


			target.AddOrUpdateItem(new CacheDefinition
			{
				Value = "testValueDifferent",
				Id = "testId"
			});
			target.Execute().Count();

			var item = target.GetItems("")["testId"];
			Assert.AreEqual("testValueDifferent", item.Value);
		}
		#endregion

		#region InvalidateItem
		[TestMethod]
		public void ItShouldBePossibleToInvalidateItem()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem("testId", "testValue");
			target.Execute().Count();

			target.InvalidateItem("testId");
			target.Execute().Count();

			Assert.IsFalse(target.GetItems("").ContainsKey("testId"));
		}


		[TestMethod]
		public void ItShouldBePossibleToInvalidateItemNoItemShouldDoNothing()
		{
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			target.InvalidateItem("testId");
			target.Execute().Count();

			Assert.IsFalse(target.GetItems("").ContainsKey("testId"));
		}
		#endregion

		#region AddAndGet

		private int ExecuteEnumerator(IEnumerator enumerator)
		{
			var result = 0;
			while (enumerator.MoveNext())
			{
				result++;
			}
			return result;
		}

		private int _loadDataCalled = 0;
		private static RunnerForTest _runner;

		private IEnumerable<ICoroutineResult> LoadDataWithWait(int waitCycleMs, int times)
		{
			_loadDataCalled++;
			while (times > 0)
			{
				Thread.Sleep(waitCycleMs);
				yield return CoroutineResult.Wait;
				times--;
			}
			yield return CoroutineResult.Return("RESULT");
		}

		private IEnumerable<ICoroutineResult> LoadData(int times, string returns = "RESULT")
		{
			_loadDataCalled++;
			while (times > 0)
			{
				yield return CoroutineResult.Wait;
				times--;
			}
			yield return CoroutineResult.Return(returns);
		}


		[TestMethod]
		public void OverlappingRequestWillNotInvokeTwiceTheLoadDataButWillHaveTheSameResult()
		{
			_loadDataCalled = 0;
			object result1 = null;
			object result2 = null;
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			_runner.StartCoroutine(new NodeCacheCoroutine(target));
			_runner.RunCycle();		//Initialize node cache coroutine

			var waiter1 = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadData(5)
			}, (r) =>
			{
				result1 = r;
			});
			_runner.StartCoroutine(waiter1.AsCoroutine());
			var waiter2 = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadData(5, "ANOTHER THING"),
				ExpireAfter = TimeSpan.FromMinutes(5)
			}, (r) =>
			{
				result2 = r;
			});
			_runner.StartCoroutine(waiter2.AsCoroutine());
			_runner.RunCycle();	//Load the coroutines
			_runner.RunCycle();	//Start the coroutines methods
			_runner.RunCycle(5);	//Wait for completion
			_runner.RunCycle();	//Copy the data

			Assert.AreEqual(1, _loadDataCalled);
			Assert.AreEqual("RESULT", result1, "Result 1 is " + result1);
			Assert.AreEqual("RESULT", result2, "Result 2 is " + result1);
		}


		[TestMethod]
		public void ItShouldBePossibleToAddAndGetAnAlreadyExistingItem()
		{
			_loadDataCalled = 0;
			object result = null;
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));
			target.AddItem("test", "RESULT");
			_runner.StartCoroutine(new NodeCacheCoroutine(target));
			_runner.RunCycle();		//Initialize node cache coroutine

			var waiter = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadData(5),
				ExpireAfter = TimeSpan.FromMinutes(1)
			}, (r) =>
			{
				result = r;
			});
			_runner.StartCoroutine(waiter.AsCoroutine());
			//target.Execute().Count();	//Initialize 
			_runner.RunCycle();

			Assert.AreEqual("RESULT", result);
			Assert.AreEqual(0, _loadDataCalled);
		}



		[TestMethod]
		public void AddAndGetNonExistingItemWithTimeoutWillNotSetTheValue()
		{
			_loadDataCalled = 0;
			object result = null;
			var target = new NodeCache(_runner,TimeSpan.FromSeconds(10));

			var waiter = (FluentResultBuilder)target.AddAndGet(new CacheDefinition
			{
				Id = "test",
				LoadData = () => LoadData(20)
			}, (r) => result = r);
			target.Execute().Count();	//Initialize 
			//Fun the getter function
			_runner.RunCycle(6);
			target.Execute().Count();	//Initialize 
			target.Execute().Count();	//Initialize 

			//ExecuteEnumerator(waiter.NestedEnumerator);
			Assert.IsNull(result);
			Assert.AreEqual(1, _loadDataCalled);
		}


		#endregion

		/*
				
		
		ItShouldBePossibleToAddAnItemWithLoadItem
		ItShouldBePossibleToAddAnItemWithLoadItemOverlappingButCallingTheRequestOnlyOnce
		ItShouldBePossibleToAddAndGet
		ItShouldBePossibleToAddAndGetWithTimeoutThatShouldDoNothing
		ItShouldBePossibleToAddAndGetNoItemShouldDoNothing
		ItShouldBePossibleToAddAndGetWithCd
		ItShouldBePossibleToAddAndGetWithTimeoutThatShouldDoNothingWithCd
		ItShouldBePossibleToAddAndGetNoItemShouldDoNothingWithCd
		ItShouldBePossibleToAddOrUpdateAndGetAdding
		ItShouldBePossibleToAddOrUpdateAndGetUpdating
		ItShouldBePossibleToAddOrUpdateAndGetAddingWithCd
		ItShouldBePossibleToAddOrUpdateAndGetUpdatingWithCd
		ItShouldBePossibleToGetAnItem
		ItShouldBePossibleToGetAnItemNoItemShouldDoNothing
		ItShouldBePossibleToInvalidateGroup
		ItShouldBePossibleToInvalidateGroupNoGroupShouldDoNothing
		 
		ItemsWithoutValueShouldBePurged
		ItemsTimeoutedShouldBePurged
		ReachingTheCappedLimitItemsShouldBePurged
		 */

		//Should test that the correct expiration is set...
		//Adding an item contextually with the group
		// ReSharper restore ReturnValueOfPureMethodIsNotUsed
	}
}
