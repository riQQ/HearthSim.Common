using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Util
{
	public static class HearthstoneProc
	{
		private static bool _waiting;

		public static async Task<string> GetExecutablePath()
		{
			Process proc;
			while((proc = GetProcess()) == null)
			{
				if(!_waiting)
				{
					_waiting = true;
					Log.Debug("Waiting for process...");
				}
				await Task.Delay(500);
			}
			var executable = new FileInfo(proc.MainModule.FileName);
			if(_waiting)
			{
				_waiting = false;
				Log.Info($"Process found! {executable.FullName}");
			}
			return executable.Directory?.FullName;
		}

		public static Process GetProcess()
		{
			try
			{
				return Process.GetProcessesByName("Hearthstone").FirstOrDefault();
			}
			catch(Exception)
			{
				return null;
			}
		}
	}
}
