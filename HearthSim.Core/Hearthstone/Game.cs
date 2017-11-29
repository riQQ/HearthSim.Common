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

		internal override void OnModeChanged(ModeChangedEventArgs args)
		{
			CurrentMode = args.CurrentMode;
			PreviousMode = args.PreviousMode;
			base.OnModeChanged(args);
		}

		internal override void OnCreateGame(GameCreatedEventArgs args)
		{
			if(CurrentGame != null)
				CurrentGame.Modified -= OnGameStateChanged;
			CurrentGame = new GameState();
			CurrentGame.Modified += OnGameStateChanged;
			base.OnCreateGame(new GameCreatedEventArgs(CurrentGame));
		}

		internal override void OnGameStateChanged(GameStateChangedEventArgs args)
		{
			base.OnGameStateChanged(args);
			if(args.Modifier is TagChange t && t.Tag == GameTag.STATE && t.Value == (int)State.COMPLETE)
				OnGameEnded();
		}

		internal override void OnHearthstoneExited()
		{
			Collection.Unload();
			Account.Unload();
			Arena.Unload();
			TavernBrawl.Unload();
			IsRunning = false;
			CurrentMode = Mode.INVALID;
			CurrentGame = null;
			Build = null;
			base.OnHearthstoneExited();
		}

		internal override void OnHearthstoneLoaded()
		{
			IsRunning = true;
			base.OnHearthstoneLoaded();
		}
	}
}
