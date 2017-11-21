using System;
using System.IO;
using System.Linq;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core
{
	public class Core
	{
		public Core(string hearthstoneDirectory, params LogWatcherData[] additionalLogReaders)
		{
			Game = new Game();
			LogParserManager = new LogParserManager();

			PowerParser = new PowerParser();
			PowerParser.CreateGame += PowerParser_CreateGame;
			PowerParser.GameStateChange += PowerParser_GameStateChange;
			LogParserManager.RegisterParser(PowerParser);

			DecksParser = new DecksParser();
			LogParserManager.RegisterParser(DecksParser);

			LoadingScreenParser = new LoadingScreenParser();
			LogParserManager.RegisterParser(LoadingScreenParser);

			LogReader = new LogReader(
				Path.Combine(hearthstoneDirectory, "Logs"),
				new []
				{
					LogWatcherConfigs.Power,
					LogWatcherConfigs.LoadingScreen,
					LogWatcherConfigs.Decks,
				}.Concat(additionalLogReaders).ToArray()
			);
			LogReader.NewLines += eventArgs => LogParserManager.Parse(eventArgs.Lines);
			LogReader.LogConfigUpdated += HearthstoneRestartRequired;
			LogReader.LogConfigUpdateFailed += LogConfigError;
		}

		public void Start() => LogReader.Start();

		public Game Game { get; }
		public PowerParser PowerParser { get; }
		public DecksParser DecksParser { get; }
		public LoadingScreenParser LoadingScreenParser { get; }
		public LogParserManager LogParserManager { get; }
		internal LogReader LogReader { get; }

		public event Action<IGameStateModifier, GameState> GameStateChanged;
		public event Action HearthstoneRestartRequired;
		public event Action<Exception> LogConfigError;

		private void PowerParser_CreateGame()
		{
			Game.CurrentGame = new GameState();
		}

		private void PowerParser_GameStateChange(IGameStateModifier mod)
		{
			if(Game.CurrentGame == null)
				return;
			Game.CurrentGame.Apply(mod);
			GameStateChanged?.Invoke(mod, Game.CurrentGame);
		}
	}
}
