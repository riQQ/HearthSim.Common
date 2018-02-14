using System;

namespace HearthSim.Core.Hearthstone
{
	public class CommonGameEventArgs : EventArgs
	{
		public int EntityId { get; }
		public IGameState GameState { get; }

		public CommonGameEventArgs(int entityId, IGameState state)
		{
			EntityId = entityId;
			GameState = state;
		}
	}
}
