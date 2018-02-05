using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameStateChangedEventArgs : System.EventArgs
	{
		public GameStateChangedEventArgs(IGameStateModifier modifier, IGameState state)
		{
			Modifier = modifier;
			State = state;
		}

		public IGameStateModifier Modifier { get; }
		public IGameState State { get; }
	}
}
