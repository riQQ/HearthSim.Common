using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.Util.Exceptions;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Util
{
	public static class HearthstoneProc
	{
		private const int MaxTries = 5;
		private static bool _waiting;

		/// <summary>
		/// Tries to obtain the Hearthstone installation path from the running Process
		/// This waits for the process to exist before returning.
		/// </summary>
		/// <exception cref="HearthstoneNotFoundException"></exception>
		/// <returns>Path of the Hearthstone installation</returns>
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
			Exception exception = null;
			for(var i = 0; i < MaxTries; i++)
			{
				try
				{
					var executable = new FileInfo(proc.MainModule.FileName);
					if(_waiting)
					{
						_waiting = false;
						Log.Info($"Process found! {executable.FullName}");
					}
					return executable.Directory?.FullName;
				}
				catch(Exception e)
				{
					exception = e;
					await Task.Delay(5000);
				}
			}
			throw new HearthstoneNotFoundException("Could not get Hearthstone installation path", exception);
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
