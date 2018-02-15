namespace HearthSim.Core.Hearthstone.Events
{
	public class EntityGameEvent : BaseGameEvent<int>
	{
		public EntityGameEvent(int data, IGameState state) : base(data, state)
		{
		}
	}
}
