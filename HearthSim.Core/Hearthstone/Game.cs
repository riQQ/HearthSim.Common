using HearthSim.Core.Hearthstone.Enums;

namespace HearthSim.Core.Hearthstone
{
	public class Game
	{
		public GameState CurrentGame { get; internal set; }
		public Mode CurrentMode { get; internal set; }
		public Mode PreviousMode { get; internal set; }
	}
}
