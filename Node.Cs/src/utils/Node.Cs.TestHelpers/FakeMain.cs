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


using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Logging;
using NodeCs.Shared;
using System.Threading;

namespace Node.Cs.TestHelpers
{
	public class FakeMain : IMain
	{
		public FakeMain(string assemblyDirectory, ICoroutinesManager coroutinesManager)
		{
			AssemblyDirectory = assemblyDirectory;
			CoroutinesManager = coroutinesManager;
		}
		public string AssemblyDirectory { get; private set; }
		public void Initialize(string[] args, string helpMessage)
		{

		}

		public bool Wait(int ms)
		{
			return false;
		}

		public void Abort()
		{

		}

		public void Start()
		{

		}

		public void Stop()
		{

		}

		public void StopIncoming()
		{

		}

		public void AllowIncoming()
		{

		}

		public bool Execute(string result)
		{
			return true;
		}

		public void RenewLease()
		{

		}

		public void AddThread(Thread thread, string threadName)
		{

		}

		public void RemoveThread(string threadName)
		{

		}

		public void RegisterCommand(string name, CommandDefinition definition)
		{

		}

		public void UnregisterCommand(string name, int paramsCount)
		{

		}

		public ICoroutinesManager CoroutinesManager { get; private set; }
		public ILogger Log { get; set; }
	}
}