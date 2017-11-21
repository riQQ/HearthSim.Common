using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.LogConfig;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.LogReading.Internal;
using HearthSim.Core.Util.Extensions;
using HearthSim.Core.Util.Logging;
using HearthSim.Core.Util.Watchers;

namespace HearthSim.Core.LogReading
{
	internal class LogReader
	{
		internal const int UpdateDelay = 100;
		private readonly List<LogWatcher> _watchers = new List<LogWatcher>();
		private bool _running;
		private bool _stop;
		private readonly ProcessWatcher _procWatcher;

		public LogReader(string logDirectory, params LogWatcherData[] logReaderInfos)
		{
			_procWatcher = new ProcessWatcher();
			_procWatcher.OnStart += proc => StartReaders();
			_procWatcher.OnExit += proc => StopReaders().Forget();
			_watchers.AddRange(logReaderInfos.Select(x => new LogWatcher(x, logDirectory)));
			LogConfigWatcher.Start();
			LogConfigUpdater.LogConfigUpdated += LogConfigUpdated;
			LogConfigUpdater.LogConfigUpdateFailed += LogConfigUpdateFailed;
		}

		public event Action<NewLinesEventArgs> NewLines;
		public event Action LogConfigUpdated;
		public event Action<Exception> LogConfigUpdateFailed;

		public void Start()
		{
			if(_running)
				return;
			Log.Debug("Starting proccess watcher");
			_procWatcher.Run();
		}

		private async void StartReaders()
		{
			if(_running)
				return;
			await LogConfigUpdater.Run(_watchers.Select(x => x.Info.Name));
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
			var loadingScreen = Analyzer.FindEntryPoint(GetLogWatcher("LoadingScreen"));
			var decks = Analyzer.FindEntryPoint(GetLogWatcher("Decks"));
			if(decks > DateTime.MinValue)
				return decks;
			if(loadingScreen > DateTime.MinValue)
				return loadingScreen;
			return power;
		}

		public async Task Stop()
		{
			if(!_running)
				return;
			await Task.WhenAll(_procWatcher.Stop(), StopReaders());
		}

		private async Task StopReaders()
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
