using System;
using HearthSim.Util.Logging;
using Microsoft.Win32;


namespace HearthSim.Util
{
	public class RegistryHelper
	{
		private static RegistryKey GetAutoRunKey() => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		public static bool SetAutoRun(string keyName, string executablePath, string args)
		{
			if(string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(executablePath))
				return false;
			var path = $"\"{executablePath}\"";
			if(!string.IsNullOrEmpty(args))
				path += " " + args;
			try
			{
				using(var key = GetAutoRunKey())
					key?.SetValue(keyName, path);
				Log.Debug("Set AutoRun path to " + path);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}

		}

		public static bool DeleteRunKey(string keyName)
		{
			try
			{
				using(var key = GetAutoRunKey())
					key?.DeleteValue(keyName, false);
				Log.Debug("Deleted AutoRun key");
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

	}
}
