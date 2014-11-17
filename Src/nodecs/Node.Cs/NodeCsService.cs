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
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CoroutinesLib;
using CoroutinesLib.Shared;
using CoroutinesLib.Shared.Enums;
using GenericHelpers;
using NodeCs.Shared;
using Timer = System.Timers.Timer;

namespace NodeCs
{
	internal class AppDomainInstance
	{
		public AppDomain Ad;
		public Main Bs;
	}
	public class NodeCsService : ServiceBase
	{
		private const int MAX_ALLOCATED_MEMORY = 1 * 1000 * 1000 * 1000;
		private readonly Queue<AppDomainInstance> _bootstrappers;
		private readonly List<AppDomainInstance> _stopping;
		private readonly Timer _timer;
		private string[] _args;
		private string _help;

		public IMain Main
		{
			get
			{
				var res = _bootstrappers.Peek();
				if (res != null) return res.Bs;
				return null;
			}
		}

		public NodeCsService()
		{
			_bootstrappers = new Queue<AppDomainInstance>();
			_stopping = new List<AppDomainInstance>();
			_timer = new Timer();
			_timer.Elapsed += OnRecycleElapsed;
			_timer.Interval = 1000;
			LifetimeServices.LeaseTime = TimeSpan.FromMinutes(10); 
			LifetimeServices.RenewOnCallTime = TimeSpan.FromMinutes(10);
			AppDomain.MonitoringIsEnabled = true;
		}

		protected override void OnStart(string[] args)
		{
			// Run the service version here 
			//  NOTE: If you're task is long running as is with most 
			//  services you should be invoking it on Worker Thread 
			//  !!! don't take to long in this function !!!
			Run(args, null);
			var bootstrapper = _bootstrappers.Peek();
			bootstrapper.Bs.AllowIncoming();
			base.OnStart(args);
		}

		protected override void OnStop()
		{
			Terminate();
			base.OnStop();
		}

		public void Terminate()
		{
			_timer.Enabled = false;
			var bootstrapper = _bootstrappers.Peek();
			bootstrapper.Bs.Stop();
			var result = bootstrapper.Bs.Wait(1000);
			if (!result)
			{
				bootstrapper.Bs.Abort();
				bootstrapper.Bs = null;
				AppDomain.Unload(bootstrapper.Ad);
			}
		}
		public void Run(string[] args, string help)
		{
			_args = args;
			_help = help;
			_timer.Enabled = true;
			var ad = AppDomain.CreateDomain("Domain-" + Guid.NewGuid());
			var bootstrapper =
				(Main)ad.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Main).FullName);
			bootstrapper.Initialize(args, help);
			var adi = new AppDomainInstance
			{
				Bs = bootstrapper,
				Ad = ad
			};
			_bootstrappers.Enqueue(adi);
			bootstrapper.Start();
		}


		private void Recycle()
		{
			var old = _bootstrappers.Dequeue();
			Run(_args, _help);
			var theNew = _bootstrappers.Peek();

			old.Bs.StopIncoming();
			Thread.Sleep(10);
			theNew.Bs.AllowIncoming();
			_stopping.Add(old);
		}

		private void OnRecycleElapsed(object sender, ElapsedEventArgs e)
		{
			if (_bootstrappers.Count == 0)
			{
				return;
			}
			for (int index = _stopping.Count - 1; index >= 0; index--)
			{
				var item = _stopping[index];
				item.Bs.Stop();
				if (item.Bs.Wait(1000))
				{
					_stopping.RemoveAt(index);
				}
			}
			var current = _bootstrappers.Peek();
			current.Bs.RenewLease();
			if (AppDomain.MonitoringSurvivedProcessMemorySize > MAX_ALLOCATED_MEMORY)
			{
				Recycle();
			}
		}

		public bool Execute(string result)
		{
			if (result.ToLowerInvariant().Trim() == "recycle")
			{
				Recycle();
			}
			else
			{
				var bootstrapper = _bootstrappers.Peek();
				return bootstrapper.Bs.Execute(result);
			}
			return true;
		}
	}
}