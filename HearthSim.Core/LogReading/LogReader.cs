using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.LogReading.Internal;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Logging;

namespace HearthSim.Core.LogReading
{
	public interface ILogInput
	{
		event Action<NewLinesEventArgs> NewLines;
	}

	internal class LogReader : ILogInput
	{
		internal const int UpdateDelay = 100;
		private readonly List<LogWatcher> _watchers = new List<LogWatcher>();
		private bool _running;
		private bool _stop;

		internal IEnumerable<string> Logs => _watchers.Select(x => x.Info.Name);

		internal LogReader(IEnumerable<LogWatcherData> logReaderInfos)
		{
			_watchers.AddRange(logReaderInfos.Select(x => new LogWatcher(x)));
		}

		public event Action<NewLinesEventArgs> NewLines;

		public async void Start(string hearthstoneDirectory)
		{
			if(_running)
				return;
			var logDirectory = Path.Combine(hearthstoneDirectory, "Logs");
			var startingPoint = GetStartingPoint(logDirectory);
			Log.Debug($"Starting log readers at {startingPoint}");
			foreach(var logReader in _watchers)
				logReader.Start(logDirectory, logReader.Info.Name == "Decks" ? startingPoint.Decks : startingPoint.Default);
			_running = true;
			_stop = false;
			var newLines = new SortedList<DateTime, List<Line>>();
			while(!_stop)
			{
				await Task.Factory.StartNew(() =>
				{
					foreach(var logReader in _watchers)
					{
						var lines = logReader.Collect();
						foreach(var line in lines)
						{
							if(!newLines.TryGetValue(line.Time, out var logLines))
								newLines.Add(line.Time, logLines = new List<Line>());
							logLines.Add(line);
						}
					}
				});
				NewLines?.Invoke(new NewLinesEventArgs(newLines.Values.SelectMany(x => x)));
				newLines.Clear();
				await Task.Delay(UpdateDelay);
			}
			_running = false;
		}

		private StartingPoint GetStartingPoint(string directory)
		{
			LogWatcher GetLogWatcher(string name) => _watchers.SingleOrDefault(x => x.Info.Name == name);
			var power = Analyzer.FindEntryPoint(directory, GetLogWatcher("Power"));
			var decks = Analyzer.FindEntryPoint(directory, GetLogWatcher("Decks"));
			var loadingScreen = Analyzer.FindEntryPoint(directory, GetLogWatcher("LoadingScreen"));
			return new StartingPoint(power, decks, loadingScreen);
		}

		public async Task Stop()
		{
			if(!_running)
				return;
			Log.Debug("Stopping log readers");
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(_watchers.Select(x => x.Stop()));
		}

		private class StartingPoint
		{
			public StartingPoint(DateTime power, DateTime decks, DateTime loadingScreen)
			{
				Power = power;
				Decks = decks;
				LoadingScreen = loadingScreen;
				Default = new[] {Power, Decks, LoadingScreen}.Max();
			}

			public DateTime Power { get; }
			public DateTime Decks { get; }
			public DateTime LoadingScreen { get; }
			public DateTime Default { get; }
		}
	}
}
