using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameEndEventArgs : System.EventArgs
	{
		public GameEndEventArgs(int? build, IGameState gameState, int wins, int losses)
		{
			Build = build;
			GameState = gameState;
			Wins = wins;
			Losses = losses;
		}

		public int? Build { get; }
		public IGameState GameState { get; }
		public int Wins { get; }
		public int Losses { get; }
	}
}
