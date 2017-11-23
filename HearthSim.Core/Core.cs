using System;
using System.IO;
using System.Linq;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogParsing.Parsers.LoadingScreen;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core
{
	public class Core
	{
		private readonly BlockHelper _blockHelper;

		public Core(string hearthstoneDirectory, params LogWatcherData[] additionalLogReaders)
		{
			Game = new Game();
			_blockHelper = new BlockHelper(Game);
			LogParserManager = new LogParserManager();

			PowerParser = new PowerParser();
			PowerParser.CreateGame += PowerParser_CreateGame;
			PowerParser.GameStateChange += PowerParser_GameStateChange;
			PowerParser.BlockStart += PowerParser_BlockStart;
			LogParserManager.RegisterParser(PowerParser);

			DecksParser = new DecksParser();
			LogParserManager.RegisterParser(DecksParser);

			LoadingScreenParser = new LoadingScreenParser();
			LoadingScreenParser.ModeChanged += LoadingScreenParser_ModeChanged;
			LogParserManager.RegisterParser(LoadingScreenParser);

			ArenaParser = new ArenaParser();
			LogParserManager.RegisterParser(ArenaParser);

			RachelleParser = new RachelleParser();
			LogParserManager.RegisterParser(RachelleParser);

			LogReader = new LogReader(
				Path.Combine(hearthstoneDirectory, "Logs"),
				new []
				{
					LogWatcherConfigs.Power,
					LogWatcherConfigs.LoadingScreen,
					LogWatcherConfigs.Decks,
					LogWatcherConfigs.Arena,
					LogWatcherConfigs.Rachelle
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
		public ArenaParser ArenaParser { get; }
		public RachelleParser RachelleParser { get; }
		public LogParserManager LogParserManager { get; }
		internal LogReader LogReader { get; }

		public event Action<IGameStateModifier, GameState> GameStateChanged;
		public event Action HearthstoneRestartRequired;
		public event Action<Exception> LogConfigError;

		private void PowerParser_CreateGame()
		{
			if(Game.CurrentGame != null)
				Game.CurrentGame.Modified -= GameStateChanged;
			Game.CurrentGame = new GameState();
			Game.CurrentGame.Modified += GameStateChanged;
		}

		private void PowerParser_GameStateChange(IGameStateModifier mod)
		{
			Game.CurrentGame?.Apply(mod);
		}

		private void PowerParser_BlockStart(BlockData block)
		{
			foreach(var cardId in _blockHelper.GetCreatedCards(block))
				block.PredictedCards.Add(cardId);
		}

		private void LoadingScreenParser_ModeChanged(ModeChangedEventArgs args)
		{
			Game.CurrentMode = args.CurrentMode;
			Game.PreviousMode = args.PreviousMode;
		}
	}
}
