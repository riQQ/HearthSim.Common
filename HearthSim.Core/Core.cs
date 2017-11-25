﻿using System.IO;
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

namespace HearthSim.Core
{
	public class Core
	{
		private readonly BlockHelper _blockHelper;
		private readonly LogReader _logReader;

		public Core(string hearthstoneDirectory, params LogWatcherData[] additionalLogReaders)
		{
			Game = new Game();
			_blockHelper = new BlockHelper(Game);
			var logParserManager = new LogParserManager();

			var powerParser = new PowerParser();
			powerParser.CreateGame += Game.OnCreateGame;
			powerParser.GameStateChange += mod => Game?.CurrentGame.Apply(mod);
			powerParser.BlockStart += PowerParser_BlockStart;
			powerParser.StartSpectator += Game.OnSpectatorStart;
			powerParser.EndSpectator += Game.OnSpectatorEnd;
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
			arenaParser.ArenaRunComplete += Game.OnArenaRunComplete;
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
		}

		public Game Game { get; }

		public void Start() => _logReader.Start();

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
				Game.Account.Update(account.Hi, account.Lo, battleTag.Name, battleTag.Number);
			}
			if(args.PreviousMode == Mode.LOGIN)
				Game.OnHearthstoneLoaded();
		}
	}
}
