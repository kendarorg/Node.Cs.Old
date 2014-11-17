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


namespace Node.Cs.Lib
{
	public static class PerfMonConst
	{
		// ReSharper disable InconsistentNaming
		public const string NodeCs_Cache_CacheItemsCount = "NodeCs.Cache.CacheItemsCount";

		public const string NodeCs_Threading_StartedCoroutines = "NodeCs.Threading.StartedCoroutines";
		public const string NodeCs_Threading_RunningCoroutines = "NodeCs.Threading.RunningCoroutines";
		public const string NodeCs_Threading_TerminatedCoroutines = "NodeCs.Threading.TerminatedCoroutines";

		public const string NodeCs_Status_Status = "NodeCs.Status.Status";
		public const string NodeCs_Status_Exceptions = "NodeCs.Status.Exceptions";

		public const string NodeCs_Network_OpenedConnections = "NodeCs.Network.OpenedConnections";
		public const string NodeCs_Network_CurrentConnections = "NodeCs.Network.CurrentConnections";
		public const string NodeCs_Network_ClosedConnections = "NodeCs.Network.ClosedConnections";
		public const string NodeCs_Network_RequestDurations = "NodeCs.Network.RequestDurations";
		// ReSharper restore InconsistentNaming
	}
}
