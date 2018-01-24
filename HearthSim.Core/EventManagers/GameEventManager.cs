using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Watchers;

namespace HearthSim.Core.EventManagers
{
	public class GameEventManager
	{
		private readonly Game _game;
		private readonly IGameDataProvider _gameData;
		private readonly ArenaWatcher _arenaWatcher;
		private readonly PackWatcher _packWatcher;

		public GameEventManager(Game game, ILogInput logInput, IGameDataProvider gameData)
		{
			_game = game;
			_gameData = gameData;

			_arenaWatcher = new ArenaWatcher(gameData);
			_arenaWatcher.RunComplete += game.Arena.OnArenaRunComplete;
			_arenaWatcher.CardPicked += game.Arena.OnArenaDraftPick;
			_arenaWatcher.ChoicesChanged += game.Arena.OnArenaDraftChoices;
			_arenaWatcher.DeckComplete += game.Arena.OnArenaDraftComplete;

			_packWatcher = new PackWatcher(gameData);
			_packWatcher.PackOpened += game.OnPackOpened;

			var logParserManager = new LogParserManager();

			var powerParser = new PowerParser(new DefaultGameInfoProvider(game));
			powerParser.CreateGame += () => game.OnCreateGame(null);
			powerParser.GameStateChange += mod => game.CurrentGame?.Apply(mod);
			powerParser.GameStateLog += args => game.CurrentGame?.AppendLog(args);
			powerParser.SetupComplete += game.OnSetupComplete;
			logParserManager.RegisterParser(powerParser);

			var decksParser = new DecksParser();
			decksParser.FindingGame += game.OnQueuedForGame;
			decksParser.EditedDeck += game.Collection.OnDeckEdited;
			decksParser.FoundDecks += game.Collection.OnDecksLoaded;
			logParserManager.RegisterParser(decksParser);

			var loadingScreenParser = new LoadingScreenParser();
			loadingScreenParser.ModeChanged += game.OnModeChanged;
			loadingScreenParser.ModeChanged += LoadingScreenParser_OnModeChanged;
			logParserManager.RegisterParser(loadingScreenParser);

			var arenaParser = new ArenaParser();
			arenaParser.ArenaRunComplete += () => _arenaWatcher.Update();
			logParserManager.RegisterParser(arenaParser);

			var rachelleParser = new RachelleParser();
			rachelleParser.DeckDeleted += game.Collection.OnDeckDeleted;
			rachelleParser.GoldProgressWins += game.OnGoldProgressWins;
			logParserManager.RegisterParser(rachelleParser);

			logInput.NewLines += eventArgs => logParserManager.Parse(eventArgs.Lines);
		}

		private void LoadingScreenParser_OnModeChanged(ModeChangedEventArgs args)
		{
			if(ShouldUpdateCollection(args.PreviousMode))
			{
				var cards = _gameData.GetCollection()
					?.GroupBy(x => x.Id)
					.Select(g => new CollectionCard(
						g.Key,
						g.FirstOrDefault(x => !x.Premium)?.Count ?? 0,
						g.FirstOrDefault(x => x.Premium)?.Count ?? 0
					)).ToList();
				if(cards?.Count > 0)
					_game.Collection.UpdateCards(cards);
			}

			if(args.PreviousMode >= Mode.LOGIN && !_game.Account.IsLoaded)
			{
				_game.OnHearthstoneLoaded();
				var battleTag = _gameData.GetBattleTag();
				var account = _gameData.GetAccountId();
				if(battleTag != null && account != null)
				{
					_game.Account.Update(account.Hi, account.Lo, battleTag.Name, battleTag.Number);
					_game.Region = (BnetRegion)((account.Hi >> 32) & 0xFF);
				}
			}

			if(args.CurrentMode == Mode.DRAFT)
				_arenaWatcher.Run();
			else
				_arenaWatcher.Stop();

			if(args.CurrentMode == Mode.PACKOPENING)
				_packWatcher.Run();
			else
				_packWatcher.Stop();

			if(args.CurrentMode == Mode.TAVERN_BRAWL)
			{
				var brawlInfo = _gameData.GetBrawlInfo();
				if(brawlInfo != null)
					_game.TavernBrawl.Update(brawlInfo);
			}

			if(args.CurrentMode == Mode.FATAL_ERROR)
				_game.OnHearthstoneExited();
		}

		private static bool ShouldUpdateCollection(Mode previousMode)
		{
			return previousMode == Mode.COLLECTIONMANAGER
					|| previousMode == Mode.PACKOPENING
					|| previousMode == Mode.LOGIN;
		}
	}
}
