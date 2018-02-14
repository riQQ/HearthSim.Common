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

	public class AttackEventArgs : EventArgs
	{
		public int Attacker { get; }
		public int Defender { get; }
		public IGameState GameState { get; }

		public AttackEventArgs(int attacker, int defender, IGameState state)
		{
			Attacker = attacker;
			Defender = defender;
			GameState = state;
		}
	}
}
