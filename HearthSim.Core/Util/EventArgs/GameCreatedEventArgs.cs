using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameCreatedEventArgs : System.EventArgs
	{
		public GameCreatedEventArgs(GameState game)
		{
			Game = game;
		}

		public GameState Game { get; }
	}
}
