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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTests
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes().OrderBy(a => a.Namespace + "." + a.Name))
			{
				if (IsTestClass(type))
				{
					try
					{
						ExecuteTests(type);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
						Debug.WriteLine(ex);
					}
				}
			}
		}

		private static IEnumerable<MethodInfo> MethodsWithAttribute<T>(Type type)
		{
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				if (Attribute.IsDefined(method, typeof(T)))
				{
					yield return method;
				}
			}
		}

		private static bool IsTestClass(Type type)
		{

			return MethodsWithAttribute<TestMethodAttribute>(type).Any();
		}

		private static void ExecuteTests(Type type)
		{
			Console.WriteLine("Executing: {0}.{1}", type.Namespace, type.Name);
			var testContext = new LongRunningTestContext();
			var classInstance = Activator.CreateInstance(type);
			var classInitialize = MethodsWithAttribute<ClassInitializeAttribute>(type).FirstOrDefault();
			if (classInitialize != null)
			{
				var property = type.GetProperty("TestContext");
				property.GetSetMethod().Invoke(classInstance, new object[] { testContext });
				classInitialize.Invoke(classInstance, new object[] { testContext });
			}

			try
			{
				foreach (var testMethod in MethodsWithAttribute<TestMethodAttribute>(type))
				{
					InvokeTestMethod(type, classInstance, testMethod);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Debug.WriteLine(ex);
			}
			finally
			{
				var classCleanup = MethodsWithAttribute<ClassCleanupAttribute>(type).FirstOrDefault();
				if (classCleanup != null)
				{
					classCleanup.Invoke(classInstance, new object[] { testContext });
				}
			}
		}

		private static void InvokeTestMethod(Type type,object classInstance, MethodInfo testMethod)
		{
			Console.WriteLine("\tExecuting Test {0}", testMethod.Name);
			var testInitialize = MethodsWithAttribute<TestInitializeAttribute>(type).FirstOrDefault();
			if (testInitialize != null)
			{
				testInitialize.Invoke(classInstance, new object[] { });
			}
			try
			{
				testMethod.Invoke(classInstance, new object[] { });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Debug.WriteLine(ex);
			}
			finally
			{
				var testCleanup = MethodsWithAttribute<TestCleanupAttribute>(type).FirstOrDefault();
				if (testCleanup != null)
				{
					testCleanup.Invoke(classInstance, new object[] { });
				}
			}
		}
	}
}
