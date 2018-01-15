using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class Game : GameEvents
	{
		internal Game()
		{
			Collection = new Collection();
			Account = new Account();
			Arena = new Arena();
			TavernBrawl = new TavernBrawl();
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

		internal override void OnModeChanged(ModeChangedEventArgs args)
		{
			CurrentMode = args.CurrentMode;
			PreviousMode = args.PreviousMode;
			base.OnModeChanged(args);
		}

		internal override void OnCreateGame(GameCreatedEventArgs args)
		{
			if(CurrentGame != null)
			{
				CurrentGame.Modified -= OnGameStateChanged;
				if(CurrentGame.GameEntity.GetTag(GameTag.STATE) != (int)State.COMPLETE)
					InvokeGameEnd(CurrentGame);
			}
			CurrentGame = new GameState();
			CurrentGame.Modified += OnGameStateChanged;
			base.OnCreateGame(new GameCreatedEventArgs(CurrentGame));
		}

		internal override void OnGameStateChanged(GameStateChangedEventArgs args)
		{
			base.OnGameStateChanged(args);
			if(args.Modifier is TagChange t && t.Tag == GameTag.STATE && t.Value == (int)State.COMPLETE)
				InvokeGameEnd(args.State);
		}

		private void InvokeGameEnd(GameState game)
		{
			var wins = 0;
			var losses = 0;
			if(game.MatchInfo.GameType == (int)GameType.GT_ARENA)
			{
				wins = Arena.Wins;
				losses = Arena.Losses;
			}
			else if(Converters.IsBrawl((GameType)game.MatchInfo.GameType))
			{
				wins = TavernBrawl.Wins;
				losses = TavernBrawl.Losses;
			}
			OnGameEnded(new GameEndEventArgs(Build, CurrentGame, wins, losses));
		}

		internal void Reset()
		{
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
			Reset();
			base.OnHearthstoneExited();
		}

		internal override void OnHearthstoneLoaded()
		{
			IsRunning = true;
			base.OnHearthstoneLoaded();
		}

		internal override void OnQueuedForGame(QueuedForGameEventArgs args)
		{
			SelectedDeck = new Deck(args.Deck);
			base.OnQueuedForGame(args);
		}

		internal override void OnSetupComplete()
		{
			CurrentGame.SetupComplete = true;
			base.OnSetupComplete();
		}
	}
}
