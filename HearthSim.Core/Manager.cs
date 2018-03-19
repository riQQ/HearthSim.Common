using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HearthSim.Core.EventManagers;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.HSReplay;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.LogReading.LogConfig;
using HearthSim.Core.Util.Watchers;
using HearthSim.Util.Extensions;
using HearthSim.Util.Logging;

namespace HearthSim.Core
{
	public class Manager
	{
		public HSReplayNet HSReplayNet { get; }
		public Game Game { get; }

		private readonly HSReplayNetEventManager _hsReplayNetEventManager;
		private readonly GameEventManager _gameEventManager;
		private readonly ProcessWatcher _procWatcher;
		private string _directory;
		private bool _running;
		private readonly LogReader _logReader;

		public Manager(HSReplayNetConfig config = null, IGameDataProvider gameDataProvider = null)
		{
			gameDataProvider = gameDataProvider ?? new HearthMirrorDataProvider();

			_logReader = new LogReader(new List<LogWatcherData>
			{
				LogWatcherConfigs.Power,
				LogWatcherConfigs.LoadingScreen,
				LogWatcherConfigs.Decks,
				LogWatcherConfigs.Arena,
				LogWatcherConfigs.Rachelle
			});

			Game = new Game(gameDataProvider);
			Game.HearthstoneExited += () => _logReader.Stop().Forget();

			_gameEventManager = new GameEventManager(Game, _logReader, gameDataProvider);

			_procWatcher = new ProcessWatcher("Hearthstone");
			_procWatcher.OnStart += ProcessWatcher_OnStart;
			_procWatcher.OnExit += ProcessWatcher_OnExit;

			LogConfigWatcher.Start();
			LogConfigUpdater.LogConfigUpdated += Game.OnHearthstoneRestartRequired;
			LogConfigUpdater.LogConfigUpdateFailed += Game.OnLogConfigError;

			if(config != null)
			{
				HSReplayNet = new HSReplayNet(config);
				_hsReplayNetEventManager = new HSReplayNetEventManager(HSReplayNet, Game);
				HSReplayNet.UpdateAccount().Forget();
			}
		}

		private void ProcessWatcher_OnStart(Process process)
		{
			Game.OnHearthstoneStarted();
			Task.Run(async () => Game.Build = await _procWatcher.ProcInfo.GetBuildNumber());
			StartLogReader(process);
		}

		private void ProcessWatcher_OnExit(Process process)
		{
			_logReader.Stop().Forget();
			Game.OnHearthstoneExited();
		}

		private async void StartLogReader(Process process)
		{
			if(_directory == null)
			{
				for(var i = 1; i < 10; i++)
				{
					await Task.Delay(1000 * i);
					try
					{
						_directory = new FileInfo(process.MainModule.FileName).Directory?.FullName;
						Log.Debug($"Found Hearthstone installation at \"{_directory}\"");
						break;
					}
					catch(Exception e)
					{
						Log.Error(e);
					}
				}
				if(_directory == null)
				{
					Log.Error("Could not find Hearthstone installation");
					return;
				}
			}
			await UpdateLogConfig();
			_logReader.Start(_directory);
		}

		public async Task UpdateLogConfig() => await LogConfigUpdater.Run(_logReader.Logs);

		public void Start(string directory = null)
		{
			if(_running)
				return;
			_running = true;
			_directory = directory;
			_procWatcher.Run();
		}

		public async Task Stop()
		{
			await _procWatcher.Stop();
			await _logReader.Stop();
			Game.Reset();
			_running = false;
		}
	}
}
