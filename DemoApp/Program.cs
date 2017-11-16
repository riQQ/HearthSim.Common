using System;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
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
				LogWatcherConfigs.FullScreenFx
			);


			var powerParser = new PowerParser();

			var parser = new LogParserManager();
			parser.RegisterParser(powerParser);

			watcher.NewLines += eventArgs => parser.Parse(eventArgs.Lines);

			watcher.Start();
			Console.ReadKey();
		}
	}
}
