using System;
using System.IO;
using HearthSim.Core.Util.Extensions;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.LogConfig
{
	internal class LogConfigWatcher
	{
		private static FileSystemWatcher _fileWatcher;

		public static void Start()
		{
			try
			{
				_fileWatcher = new FileSystemWatcher(LogConfigConstants.HearthstoneAppData, LogConfigConstants.LogConfigFile)
				{
					EnableRaisingEvents = true
				};
				_fileWatcher.Changed += (sender, args) => LogConfigUpdater.Run().Forget();
				_fileWatcher.Deleted += (sender, args) => LogConfigUpdater.Run().Forget();
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Pause()
		{
			if(_fileWatcher != null)
				_fileWatcher.EnableRaisingEvents = false;
		}

		public static void Continue()
		{
			if(_fileWatcher != null)
				_fileWatcher.EnableRaisingEvents = true;
		}
	}
}
