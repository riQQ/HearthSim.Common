﻿using System;
using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.Hearthstone.Events;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Logging;
using DeckType = HearthSim.Core.Hearthstone.Enums.DeckType;

namespace HearthSim.Core.Hearthstone
{
	public class Game : GlobalGameEvents
	{
		public IGameDataProvider GameDataProvider { get; }

		public Game(IGameDataProvider gameDataProvider)
		{
			GameDataProvider = gameDataProvider;
			Collection = new Collection();
			Account = new Account();
			Arena = new Arena();
			TavernBrawl = new TavernBrawl();
			GameStateEvents = new GameStateEvents();
			GameStateEvents.BlockStart += OnBlockStart;
		}

		public GameState CurrentGame { get; private set; }
		public Mode CurrentMode { get; private set; }
		public Mode PreviousMode { get; private set; }
		public Collection Collection { get; }
		public Account Account { get; }
		public bool IsRunning { get; private set; }
		public Arena Arena { get; }
		public TavernBrawl TavernBrawl { get; }
		public int? Build { get; internal set; }
		public Deck SelectedDeck { get; private set; }
		public BnetRegion Region { get; set; }
		public GameStateEvents GameStateEvents { get; }

		public bool IsInMenu => CurrentMode > Mode.LOGIN && CurrentMode != Mode.FATAL_ERROR
														&& CurrentMode != Mode.GAMEPLAY;

		internal override void OnModeChanged(ModeChangedEventArgs args)
		{
			CurrentMode = args.CurrentMode;
			PreviousMode = args.PreviousMode;
			Log.Debug($"{PreviousMode} => {CurrentMode}");
			if(PreviousMode == Mode.GAMEPLAY && CurrentGame != null)
			{
				CurrentGame.Modified -= OnGameStateChanged;
				CurrentGame.LocalPlayer.DeckChanged -= OnActivePlayerDeckChanged;
				CurrentGame.OpposingPlayer.DeckChanged -= OnActivePlayerDeckChanged;
				CurrentGame.Ready -= OnGameStarted;
				CurrentGame = null;
			}

			base.OnModeChanged(args);
		}

		internal override void OnCreateGame(GameCreatedEventArgs args)
		{
			if(CurrentGame != null && CurrentGame.GameEntity.GetTag(GameTag.STATE) != (int)State.COMPLETE)
				InvokeGameEnd(CurrentGame);
			CurrentGame = new GameState(GameDataProvider, GameStateEvents);
			CurrentGame.Modified += OnGameStateChanged;
			CurrentGame.LocalPlayer.DeckChanged += OnActivePlayerDeckChanged;
			CurrentGame.OpposingPlayer.DeckChanged += OnActivePlayerDeckChanged;
			CurrentGame.Ready += OnGameStarted;
			Log.Debug($"{CurrentGame.MatchInfo?.LocalPlayer.Name ?? "unknown"} "
					+ $"vs {CurrentGame.MatchInfo?.OpposingPlayer.Name ?? "unknown"} "
					+ $"GameType={(GameType)(CurrentGame.MatchInfo?.GameType ?? 0)} "
					+ $"Format={(FormatType)(CurrentGame.MatchInfo?.FormatType ?? 0)}");
			base.OnCreateGame(new GameCreatedEventArgs(CurrentGame));
		}

		internal void OnGameStateChanged(GameStateChangedEventArgs args)
		{
			if(args.Modifier is TagChange t && (t.Tag == GameTag.STATE && t.Value == (int) State.COMPLETE))
				InvokeGameEnd(args.State);
		}

		private void InvokeGameEnd(IGameState game)
		{
			var wins = 0;
			var losses = 0;
			var matchInfo = game.MatchInfo;
			if(matchInfo != null)
			{
				if(matchInfo.GameType == (int)GameType.GT_ARENA)
				{
					wins = Arena.Wins;
					losses = Arena.Losses;
				}
				else if(Converters.IsBrawl((GameType)matchInfo.GameType))
				{
					wins = TavernBrawl.Wins;
					losses = TavernBrawl.Losses;
				}
			}

			Log.Debug($"{matchInfo?.LocalPlayer.Name ?? "unknown"} "
					+ $"vs {matchInfo?.OpposingPlayer.Name ?? "unknown"} "
					+ $"ended ({(PlayState)CurrentGame.LocalPlayerEntity.GetTag(GameTag.PLAYSTATE)})");

			OnGameEnded(new GameEndEventArgs(Build, Region, CurrentGame, wins, losses));
		}

		internal void Reset()
		{
			Log.Debug("Unloading and resettings everything");
			Collection.Unload();
			Account.Unload();
			Arena.Unload();
			TavernBrawl.Unload();
			IsRunning = false;
			CurrentMode = Mode.INVALID;
			CurrentGame = null;
			Build = null;
		}

		internal override void OnHearthstoneExited()
		{
			Log.Debug("Hearthstone exited");
			Reset();
			base.OnHearthstoneExited();
		}

		internal override void OnHearthstoneLoaded()
		{
			Log.Debug("Hearthstone is running");
			IsRunning = true;
			base.OnHearthstoneLoaded();
		}

		internal override void OnQueuedForGame(QueuedForGameEventArgs args)
		{
			SelectedDeck = args.Deck != null ? new Deck(DeckType.Constructed, args.Deck) : null;
			Log.Debug("Set SelectedDeck=" + SelectedDeck);
			base.OnQueuedForGame(args);
		}

		internal void OnSetupComplete()
		{
			Log.Debug("Setup complete");
			CurrentGame.SetupComplete = true;
			GameStateEvents.OnSetupComplete();
		}

		internal override void OnDungeonRunMatchStarted(DungeonRunMatchStartedEventArgs args)
		{
			if(args.IsNew && args.Deck != null)
			{
				SelectedDeck = args.Deck;
				Log.Debug("Set SelectedDeck=" + SelectedDeck);
			}
			base.OnDungeonRunMatchStarted(args);
		}

		internal override void OnDungeonRunDeckUpdated(DungeonRunDeckUpdatedEventArgs args)
		{
			SelectedDeck = args.Deck;
			Log.Debug("Set SelectedDeck=" + SelectedDeck);
			base.OnDungeonRunDeckUpdated(args);
		}

		internal override void OnGameEnded(GameEndEventArgs args)
		{
			Log.Debug("Clearing selected deck");
			SelectedDeck = null;
			base.OnGameEnded(args);
		}

		internal void OnGameTimeChanged(DateTime time)
		{
			if(CurrentGame != null)
				CurrentGame.GameTime.Time = time;
		}

		private void OnBlockStart(BlockGameEvent e)
		{
			if(e.Data.Type == BlockType.TRIGGER && e.Data.CardId == CardIds.Collectible.Neutral.EmperorThaurissan)
			{
				foreach(var entity in e.GameState.CurrentPlayer.InHand)
					entity.Info.CostReduction++;
			}
		}
	}
}
