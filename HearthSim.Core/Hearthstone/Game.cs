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
		}

		public GameState CurrentGame { get; private set; }
		public Mode CurrentMode { get; private set; }
		public Mode PreviousMode { get; private set; }
		public Collection Collection { get; }
		public Account Account { get; }
		public bool IsRunning { get; private set; }

		internal override void OnModeChanged(ModeChangedEventArgs args)
		{
			CurrentMode = args.CurrentMode;
			PreviousMode = args.PreviousMode;
			base.OnModeChanged(args);
		}

		internal override void OnCreateGame()
		{
			if(CurrentGame != null)
				CurrentGame.Modified -= OnGameStateChanged;
			CurrentGame = new GameState();
			CurrentGame.Modified += OnGameStateChanged;
			base.OnCreateGame();
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
			IsRunning = false;
			CurrentMode = Mode.INVALID;
			base.OnHearthstoneExited();
		}

		internal override void OnHearthstoneLoaded()
		{
			IsRunning = true;
			base.OnHearthstoneLoaded();
		}
	}
}
