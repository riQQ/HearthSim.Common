using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.Util.Exceptions;
using HearthSim.Util.Logging;

namespace HearthSim.Core.Util
{
	public class ProcInfo
	{
		public string Name { get; }
		private const int MaxTries = 5;
		private static bool _waiting;

		/// <param name="procName">Name of the process</param>
		/// <exception cref="ArgumentException">procName can not be null</exception>
		public ProcInfo(string procName)
		{
			if(procName == null)
				throw new ArgumentException($"{nameof(procName)} can not be null");
			Name = procName.Replace(".exe", "");
		}

		/// <summary>
		/// Tries to obtain the executable from the running Process.
		/// This waits for the process to exist.
		/// </summary>
		/// <exception cref="ProcessNotFoundException"></exception>
		/// <returns>Path of the executable</returns>
		public async Task<FileInfo> GetExecutable()
		{
			Process proc;
			while((proc = GetProcess()) == null)
			{
				if(!_waiting)
				{
					_waiting = true;
					Log.Debug($"Waiting for {Name} process...");
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
						Log.Debug($"Process found! {executable.FullName}");
					}
					return executable;
				}
				catch(Exception e)
				{
					exception = e;
					await Task.Delay(5000);
				}
			}
			throw new ProcessNotFoundException($"Could not get executable for {Name} process", exception);
		}

		/// <summary>
		/// Tries to obtain the executable directory from the running Process.
		/// This waits for the process to exist.
		/// </summary>
		/// <exception cref="ProcessNotFoundException"></exception>
		/// <returns>Directory of the executable</returns>
		public async Task<string> GetExecutableDirectory()
		{
			return (await GetExecutable())?.Directory?.FullName;
		}

		/// <summary>
		/// Tries to obtain the executable version info from the running Process.
		/// This waits for the process to exist.
		/// </summary>
		/// <returns>Version info of the executable</returns>
		public async Task<FileVersionInfo> GetExecutableVersion()
		{
			try
			{
				var exe = await GetExecutable();
				return FileVersionInfo.GetVersionInfo(exe.FullName);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		/// <summary>
		/// Tries to obtain the executable build number from the running Process.
		/// This waits for the process to exist.
		/// </summary>
		/// <returns>Build number of the executable</returns>
		public async Task<int?> GetBuildNumber()
		{
			return (await GetExecutableVersion())?.FilePrivatePart;
		}

		public Process GetProcess()
		{
			try
			{
				return Process.GetProcessesByName(Name).FirstOrDefault();
			}
			catch(Exception)
			{
				return null;
			}
		}
	}
}
