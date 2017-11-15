using System;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.Logging;

namespace DemoApp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Log.Initialize("D:/", "test");
			var watcher =
				new LogReader(
					new[]
					{
						new LogWatcherData("Power", new[] {"tag=GOLD_REWARD_STATE", "End Spectator"},
							s => s.StartsWith("PowerTaskList.DebugPrintPower") || s.StartsWith("GameState."),
							s => s.Contains("Spectating") || s.Contains("Spectator")),
						new LogWatcherData("LoadingScreen", new[] {"Gameplay.Start"},
							s => s.StartsWith("LoadingScreen.OnSceneLoaded") || s.StartsWith("Gameplay."))
					}, "E:\\Program Files (x86)\\Hearthstone\\Logs");
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
