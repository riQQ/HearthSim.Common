using System;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Watchers;
using HearthSim.Util.Extensions;

namespace HearthSim.Core.EventManagers
{
	public class GameEventManager
	{
		private readonly Game _game;
		private readonly IGameDataProvider _gameData;
		private readonly ArenaWatcher _arenaWatcher;
		private readonly PackWatcher _packWatcher;
		private readonly DungeonRunWatcher _dungeonRunWatcher;

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

			_dungeonRunWatcher = new DungeonRunWatcher(new DungeonRunData(game, gameData));
			_dungeonRunWatcher.DungeonRunMatchStarted += game.OnDungeonRunMatchStarted;
			_dungeonRunWatcher.DungeonRunDeckUpdated += game.OnDungeonRunDeckUpdated;

			var logParserManager = new LogParserManager();

			var powerParser = new PowerParser(new DefaultGameInfoProvider(game));
			powerParser.CreateGame += () => game.OnCreateGame(null);
			powerParser.PowerTaskListLog += args => game.OnGameTimeChanged(args.Line.Time);
			powerParser.GameStateChange += mod => game.CurrentGame?.Apply(mod);
			powerParser.BlockStart += block => game.GameStateEvents.OnBlockStart(block, game.CurrentGame);
			powerParser.BlockEnd += block => game.GameStateEvents.OnBlockEnd(block, game.CurrentGame);
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

		private async void LoadingScreenParser_OnModeChanged(ModeChangedEventArgs args)
		{
			if(args.PreviousMode >= Mode.LOGIN && !_game.Account.IsLoaded)
			{
				_game.OnHearthstoneLoaded();
				var battleTag = await Retry(_gameData.GetBattleTag);
				var account = await Retry(_gameData.GetAccountId);
				if(battleTag != null && account != null)
				{
					_game.Account.Update(account.Hi, account.Lo, battleTag.Name, battleTag.Number);
					_game.Region = (BnetRegion)((account.Hi >> 32) & 0xFF);
				}
			}

			if(ShouldUpdateCollection(args.PreviousMode, args.PreviousMode))
			{
				var collection = await Retry(_gameData.GetFullCollection);
				if(collection != null)
				{
					var cards = collection.Cards
						.GroupBy(x => x.Id)
						.Select(g => new CollectionCard(g.Key,
							g.FirstOrDefault(x => !x.Premium)?.Count ?? 0,
							g.FirstOrDefault(x => x.Premium)?.Count ?? 0
						)).ToList();
					var favoriteHeroes = collection.FavoriteHeroes.Values.Select(x =>
						new CollectionCard(x.Id, x.Premium ? 0 : 1, x.Premium ? 1 : 0))
						.ToList();
					_game.Collection.Update(cards, collection.CardBacks, favoriteHeroes,
						collection.FavoriteCardBack, collection.Dust, collection.Gold);
				}
			}

			if(args.CurrentMode == Mode.DRAFT)
				_arenaWatcher.Run();
			else
				_arenaWatcher.Stop().Forget();

			if(args.CurrentMode == Mode.PACKOPENING)
				_packWatcher.Run();
			else
				_packWatcher.Stop().Forget();

			if(args.CurrentMode == Mode.ADVENTURE || args.PreviousMode == Mode.ADVENTURE && args.CurrentMode == Mode.GAMEPLAY)
				_dungeonRunWatcher.Run();
			else
				_dungeonRunWatcher.Stop().Forget();

			if(args.CurrentMode == Mode.TAVERN_BRAWL)
			{
				var brawlInfo = _gameData.GetBrawlInfo();
				if(brawlInfo != null)
					_game.TavernBrawl.Update(brawlInfo);
			}

			if(args.CurrentMode == Mode.FATAL_ERROR)
				_game.OnHearthstoneExited();
		}

		private static bool ShouldUpdateCollection(Mode previousMode, Mode currentMode)
		{
			return previousMode == Mode.COLLECTIONMANAGER
					|| currentMode == Mode.COLLECTIONMANAGER
					|| previousMode == Mode.PACKOPENING;
		}

		private static async Task<T> Retry<T>(Func<T> func, int delay = 2000)
		{
			var value = func();
			if(value != null)
				return value;
			await Task.Delay(delay);
			return func();
		}
	}
}
