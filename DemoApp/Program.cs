using System;
using HearthSim.Core.LogReading;
using HearthSim.Core.Util.Logging;

namespace DemoApp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Log.Initialize("D:/", "test");
			var watcher = new LogReader(
				"E:\\Program Files (x86)\\Hearthstone\\Logs",
				LogWatcherConfigs.Power,
				LogWatcherConfigs.LoadingScreen,
				LogWatcherConfigs.Arena,
				LogWatcherConfigs.FullScreenFx,
				LogWatcherConfigs.Rachelle
			);
			watcher.NewLines += s =>
			{
				//foreach(var line in s.Lines)
					//Console.WriteLine(line.Text);
			};
			watcher.Start();
			Console.ReadKey();
		}

	}
}
