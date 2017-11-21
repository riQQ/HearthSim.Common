using System;
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
		public Core(string logDirectory, params LogWatcherData[] additionalLogReaders)
		{
			Game = new Game();
			PowerParser = new PowerParser();
			PowerParser.CreateGame += PowerParser_CreateGame;
			PowerParser.GameStateChange += PowerParser_GameStateChange;

			LogParserManager = new LogParserManager();
			LogParserManager.RegisterParser(PowerParser);

			LogReader = new LogReader(
				logDirectory,
				new []
				{
					LogWatcherConfigs.Power,
					LogWatcherConfigs.LoadingScreen,
					LogWatcherConfigs.FullScreenFx,
				}.Concat(additionalLogReaders).ToArray()
			);
			LogReader.NewLines += eventArgs => LogParserManager.Parse(eventArgs.Lines);
			LogReader.Start();
		}

		public Game Game { get; }
		public PowerParser PowerParser { get; }
		public LogParserManager LogParserManager { get; }
		internal LogReader LogReader { get; }

		public event Action<IGameStateModifier, GameState> GameStateChanged;

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
