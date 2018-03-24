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
		public LogReader LogReader { get; }

		private readonly HSReplayNetEventManager _hsReplayNetEventManager;
		private readonly GameEventManager _gameEventManager;
		private readonly ProcessWatcher _procWatcher;
		private string _hearthstonePath;
		private string _logDirectory;
		private bool _running;

		public Manager(HSReplayNetConfig config = null, IGameDataProvider gameDataProvider = null)
		{
			gameDataProvider = gameDataProvider ?? new HearthMirrorDataProvider();

			LogReader = new LogReader(new List<LogWatcherData>
			{
				LogWatcherConfigs.Power,
				LogWatcherConfigs.LoadingScreen,
				LogWatcherConfigs.Decks,
				LogWatcherConfigs.Arena,
				LogWatcherConfigs.Rachelle
			});

			Game = new Game(gameDataProvider);
			Game.HearthstoneExited += () => LogReader.Stop().Forget();

			_gameEventManager = new GameEventManager(Game, LogReader, gameDataProvider);

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
			LogReader.Stop().Forget();
			Game.OnHearthstoneExited();
		}

		private async void StartLogReader(Process process)
		{
			if(_hearthstonePath == null)
			{
				for(var i = 1; i < 10; i++)
				{
					await Task.Delay(1000 * i);
					try
					{
						_hearthstonePath = new FileInfo(process.MainModule.FileName).Directory?.FullName;
						Log.Debug($"Found Hearthstone installation at \"{_hearthstonePath}\"");
						break;
					}
					catch(Exception e)
					{
						Log.Error(e);
					}
				}
				if(_hearthstonePath == null)
				{
					Log.Error("Could not find Hearthstone installation");
					Game.OnHearthstoneInstallationNotFound();
					return;
				}
			}
			await UpdateLogConfig();
			LogReader.Start(_hearthstonePath, _logDirectory);
		}

		public async Task UpdateLogConfig() => await LogConfigUpdater.Run(LogReader.Logs);

		public void Start(string hearthstonePath = null, string logDirectory = "Logs")
		{
			if(_running)
				return;
			_running = true;
			_hearthstonePath = hearthstonePath;
			_logDirectory = logDirectory;
			_procWatcher.Run();
		}

		public async Task Stop()
		{
			await _procWatcher.Stop();
			await LogReader.Stop();
			Game.Reset();
			_running = false;
		}
	}
}
