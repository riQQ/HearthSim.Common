using System;
using System.Diagnostics;
using HearthSim.Util.Logging;

namespace HearthSim.Core.Util.Watchers
{
	public class ProcessWatcher : Watcher
	{
		public Action<Process> OnStart;
		public Action<Process> OnExit;
		public ProcInfo ProcInfo { get; }
		private Process _proc;

		public ProcessWatcher(string procName)
		{
			ProcInfo = new ProcInfo(procName);
		}

		public override UpdateResult Update()
		{
			try
			{
				var proc = ProcInfo.GetProcess();
				if(proc?.Id != _proc?.Id)
				{
					if(proc == null)
					{
						Log.Debug($"{ProcInfo.Name} process {_proc.Id} exited");
						OnExit?.Invoke(_proc);
					}
					else
					{
						Log.Debug($"{ProcInfo.Name} process {proc.Id} started");
						OnStart?.Invoke(proc);
					}
					_proc = proc;
				}
			}
			catch(Exception)
			{
			}
			return UpdateResult.Continue;
		}

		protected override void Reset()
		{
			_proc = null;
		}
	}
}
