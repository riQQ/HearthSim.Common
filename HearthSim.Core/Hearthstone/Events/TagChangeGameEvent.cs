using HearthSim.Core.Hearthstone.GameStateModifiers;

namespace HearthSim.Core.Hearthstone.Events
{
	public class TagChangeGameEvent : BaseGameEvent<TagChange>
	{
		public TagChangeGameEvent(TagChange data, IGameState state) : base(data, state)
		{
		}
	}
}
