using System.Diagnostics;
using System.IO;
using System.Linq;
using HearthMirror;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Extensions;
using HearthSim.Core.Util.Watchers;
using HearthSim.Core.Util.Watchers.ArenaWatcher;
using HearthSim.Core.Util.Watchers.PackWatcher;

namespace HearthSim.Core
{
	public class Core
	{
		private readonly BlockHelper _blockHelper;
		private readonly LogReader _logReader;
		private readonly ProcessWatcher _procWatcher;
		private readonly ArenaWatcher _arenaWatcher;
		private readonly PackWatcher _packWatcher;

		public Core(string hearthstoneDirectory, params LogWatcherData[] additionalLogReaders)
		{
			Game = new Game();
			_blockHelper = new BlockHelper(Game);
			var logParserManager = new LogParserManager();

			var powerParser = new PowerParser();
			powerParser.CreateGame += () => Game.OnCreateGame(null);
			powerParser.GameStateChange += mod => Game.CurrentGame?.Apply(mod);
			powerParser.BlockStart += PowerParser_BlockStart;
			powerParser.GameStateLog += args => Game.CurrentGame?.AppendLog(args);
			logParserManager.RegisterParser(powerParser);

			var decksParser = new DecksParser();
			decksParser.FindingGame += Game.OnQueuedForGame;
			decksParser.EditedDeck += Game.Collection.OnDeckEdited;
			decksParser.FoundDecks += Game.Collection.OnDecksLoaded;
			logParserManager.RegisterParser(decksParser);

			var loadingScreenParser = new LoadingScreenParser();
			loadingScreenParser.ModeChanged += Game.OnModeChanged;
			loadingScreenParser.ModeChanged += LoadingScreenParser_OnModeChanged;
			logParserManager.RegisterParser(loadingScreenParser);

			var arenaParser = new ArenaParser();
			arenaParser.ArenaRunComplete += ArenaParser_OnArenaRunComplete;
			logParserManager.RegisterParser(arenaParser);

			var rachelleParser = new RachelleParser();
			rachelleParser.DeckDeleted += Game.Collection.OnDeckDeleted;
			rachelleParser.GoldProgressWins += Game.OnGoldProgressWins;
			logParserManager.RegisterParser(rachelleParser);

			_logReader = new LogReader(
				Path.Combine(hearthstoneDirectory, "Logs"),
				new[]
				{
					LogWatcherConfigs.Power,
					LogWatcherConfigs.LoadingScreen,
					LogWatcherConfigs.Decks,
					LogWatcherConfigs.Arena,
					LogWatcherConfigs.Rachelle
				}.Concat(additionalLogReaders).ToArray()
			);
			_logReader.NewLines += eventArgs => logParserManager.Parse(eventArgs.Lines);
			_logReader.LogConfigUpdated += Game.OnHearthstoneRestartRequired;
			_logReader.LogConfigUpdateFailed += Game.OnLogConfigError;

			_procWatcher = new ProcessWatcher();
			_procWatcher.OnStart += ProcessWatcher_OnStart;
			_procWatcher.OnExit += ProcessWatcher_OnExit;

			_arenaWatcher = new ArenaWatcher(new HearthMirrorArenaProvider());
			_arenaWatcher.RunComplete += Game.Arena.OnArenaRunComplete;
			_arenaWatcher.CardPicked += Game.Arena.OnArenaDraftPick;
			_arenaWatcher.ChoicesChanged += Game.Arena.OnArenaDraftChoices;
			_arenaWatcher.DeckComplete += Game.Arena.OnArenaDraftComplete;

			_packWatcher = new PackWatcher(new HearthMirrorPackProvider());
			_packWatcher.PackOpened += Game.OnPackOpened;
		}

		private void ArenaParser_OnArenaRunComplete()
		{
			_arenaWatcher.Update();
		}

		private void ProcessWatcher_OnStart(Process process)
		{
			Game.OnHearthstoneStarted();
			Task.Run(async () => Game.Build = await HearthstoneProc.GetHearthstoneBuild());
			_logReader.Start();
		}

		private void ProcessWatcher_OnExit(Process process)
		{
			_logReader.Stop().Forget();
			Game.OnHearthstoneExited();
		}

		public Game Game { get; }

		public void Start() => _procWatcher.Run();

		private void PowerParser_BlockStart(BlockData block)
		{
			foreach(var cardId in _blockHelper.GetCreatedCards(block))
				block.PredictedCards.Add(cardId);
		}

		private void LoadingScreenParser_OnModeChanged(ModeChangedEventArgs args)
		{
			if(args.PreviousMode == Mode.COLLECTIONMANAGER || args.PreviousMode == Mode.PACKOPENING || args.PreviousMode == Mode.LOGIN)
			{
				var cards = Reflection.GetCollection()
					?.GroupBy(x => x.Id)
					.Select(g => new CollectionCard(
						g.Key,
						g.FirstOrDefault(x => !x.Premium)?.Count ?? 0,
						g.FirstOrDefault(x => x.Premium)?.Count ?? 0
					)).ToList();
				if(cards?.Count > 0)
					Game.Collection.UpdateCards(cards);
			}

			if(args.PreviousMode >= Mode.LOGIN && !Game.Account.IsLoaded)
			{
				var battleTag = Reflection.GetBattleTag();
				var account = Reflection.GetAccountId();
				if(battleTag != null && account != null)
					Game.Account.Update(account.Hi, account.Lo, battleTag.Name, battleTag.Number);
			}

			if(args.PreviousMode == Mode.LOGIN)
				Game.OnHearthstoneLoaded();

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
				var brawlInfo = Reflection.GetBrawlInfo();
				if(brawlInfo != null)
					Game.TavernBrawl.Update(brawlInfo);
			}
		}
	}
}
