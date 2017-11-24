using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameStateChangedEventArgs : System.EventArgs
	{
		public GameStateChangedEventArgs(IGameStateModifier modifier, GameState state)
		{
			Modifier = modifier;
			State = state;
		}

		public IGameStateModifier Modifier { get; }
		public GameState State { get; }
	}
}
