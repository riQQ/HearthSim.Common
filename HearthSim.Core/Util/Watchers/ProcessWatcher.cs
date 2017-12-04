using System;
using System.Diagnostics;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Util.Watchers
{
	public class ProcessWatcher : Watcher
	{
		public Action<Process> OnStart;
		public Action<Process> OnExit;
		private Process _proc;

		public override void Update()
		{
			try
			{
				var proc = HearthstoneProc.GetProcess();
				if(proc?.Id != _proc?.Id)
				{
					if(proc == null)
					{
						Log.Debug($"Hearthstone process {_proc.Id} exited");
						OnExit?.Invoke(_proc);
					}
					else
					{
						Log.Debug($"Hearthstone process {proc.Id} started");
						OnStart?.Invoke(proc);
					}
					_proc = proc;
				}
			}
			catch(Exception)
			{
			}
		}

		protected override void Reset()
		{
			_proc = null;
		}
	}
}
