using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameCreatedEventArgs : System.EventArgs
	{
		public GameCreatedEventArgs(IGameState game)
		{
			Game = game;
		}

		public IGameState Game { get; }
	}
}
