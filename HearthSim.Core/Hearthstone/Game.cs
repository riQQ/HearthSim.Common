namespace HearthSim.Core.Hearthstone
{
	public class Game
	{
		public Game()
		{
			State = new GameState();
		}

		public GameState State { get; }
	}
}
