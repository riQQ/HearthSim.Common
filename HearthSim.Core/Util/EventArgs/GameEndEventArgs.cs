using HearthDb.Enums;
using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameEndEventArgs : System.EventArgs
	{
		public GameEndEventArgs(int? build, BnetRegion region, IGameState gameState, int wins, int losses)
		{
			Build = build;
			Region = region;
			GameState = gameState;
			Wins = wins;
			Losses = losses;
		}

		public int? Build { get; }
		public IGameState GameState { get; }
		public int Wins { get; }
		public int Losses { get; }
		public BnetRegion Region { get; }
	}
}
