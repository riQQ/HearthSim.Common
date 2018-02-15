namespace HearthSim.Core.Hearthstone.Events
{
	public class AttackGameEvent : BaseGameEvent<AttackGameEvent.AttackData>
	{
		public class AttackData
		{
			public AttackData(int attacker, int defender)
			{
				Attacker = attacker;
				Defender = defender;
			}

			public int Attacker { get; }
			public int Defender { get; }
		}

		public AttackGameEvent(AttackData data, IGameState state) : base(data, state)
		{
		}

		public AttackGameEvent(int attacker, int defender, IGameState state) : base(new AttackData(attacker, defender), state)
		{
		}
	}
}
