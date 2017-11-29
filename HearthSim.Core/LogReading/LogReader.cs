using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.LogConfig;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.LogReading.Internal;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.LogReading
{
	internal class LogReader
	{
		internal const int UpdateDelay = 100;
		private readonly List<LogWatcher> _watchers = new List<LogWatcher>();
		private bool _running;
		private bool _stop;

		internal LogReader(string logDirectory, params LogWatcherData[] logReaderInfos)
		{
			_watchers.AddRange(logReaderInfos.Select(x => new LogWatcher(x, logDirectory)));
			LogConfigWatcher.Start();
			LogConfigUpdater.LogConfigUpdated += LogConfigUpdated;
			LogConfigUpdater.LogConfigUpdateFailed += LogConfigUpdateFailed;
		}

		public event Action<NewLinesEventArgs> NewLines;
		public event Action LogConfigUpdated;
		public event Action<LogConfigErrorEventArgs> LogConfigUpdateFailed;

		public async Task UpdateLogConfig() => await LogConfigUpdater.Run(_watchers.Select(x => x.Info.Name));

		public async void Start()
		{
			if(_running)
				return;
			await UpdateLogConfig();
			var startingPoint = GetStartingPoint();
			Log.Debug($"Starting log readers at {startingPoint}");
			foreach(var logReader in _watchers)
				logReader.Start(startingPoint);
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

		private DateTime GetStartingPoint()
		{
			LogWatcher GetLogWatcher(string name) => _watchers.SingleOrDefault(x => x.Info.Name == name);
			var power = Analyzer.FindEntryPoint(GetLogWatcher("Power"));
			var decks = Analyzer.FindEntryPoint(GetLogWatcher("Decks"));
			return decks > power ? decks : power;
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
	}
}
