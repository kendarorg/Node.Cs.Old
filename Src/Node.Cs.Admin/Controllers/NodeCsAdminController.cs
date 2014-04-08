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


using System.Collections.Generic;
using ConcurrencyHelpers.Monitor;
using Node.Cs.Admin.Attributes;
using Node.Cs.Admin.Models;
using Node.Cs.Lib.Controllers;

namespace Node.Cs.Admin.Controllers
{
	[ShouldBeLocal]
	public class NodeCsAdminController : ControllerBase
	{
		public IEnumerable<IResponse> Index()
		{
			var root = SetupPerfMonTree();
			yield return View(root);
		}

		private PerfMonItem SetupPerfMonTree()
		{
			var root = new PerfMonItem() { Name = "Root" };
			var data = PerfMon.Data;

			foreach (var item in data)
			{
				var current = root;
				var path = item.Id.Split('.');
				var i = 0;
				for (; i < (path.Length); i++)
				{
					var curPath = path[i];
					if (!current.ChildItems.ContainsKey(curPath))
					{
						var newpfo = new PerfMonItem() { Name = curPath };
						current.ChildItems.Add(curPath, newpfo);
					}
					current = current.ChildItems[curPath];
				}
				ParseData(current, item);
			}
			return root;
		}

		private string ParseData(PerfMonItem current, BaseMetric item)
		{
			var sm = item as StatusMetric;
			if (sm != null)
			{
				current.Data = string.Format("Timestamp:{0} Status:{1} Start:{2}",
					sm.LastTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"),
					sm.Status.ToString(),
					sm.StartTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"));
				current.ShortData = sm.Status.ToString();
			}
			var vm = item as ValueCounterMetric;
			if (vm != null)
			{
				current.Data = string.Format("Timestamp:{0} Value:{1} Min:{2} Max:{3} Avg:{4} Start:{5}",
					vm.LastTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"),
					vm.Value,
					vm.Max < 0 ? -1 : vm.Min,
					vm.Max < 0 ? -1 : vm.Max,
					vm.Max < 0 ? -1 : vm.Avg,
					vm.StartTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"));
				current.ShortData = vm.Avg.ToString();
			}
			var re = item as RunAndExcutionCounterMetric;
			if (re != null)
			{
				current.Data = string.Format("Timestamp:{0} Run:{1} RunAvg:{2} Elapsed:{3}" +
					"Min:{4} Max:{5} Avg:{6} Start:{7}",
					re.LastTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"),
					re.RunValue,
					re.RunAvg,
					re.ElapsedValue,
					re.ElapsedMax < 0 ? -1 : re.ElapsedMin,
					re.ElapsedMax < 0 ? -1 : re.ElapsedMax,
					re.ElapsedMax < 0 ? -1 : re.ElapsedAvg,
					re.StartTimestamp.ToString("yyyy/MM/dd-HH:mm:ss"));
				current.ShortData = re.RunAvg + "-" + re.ElapsedAvg;
			}
			return string.Empty;
		}
	}
}
