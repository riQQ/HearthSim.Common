using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class HideEntity : IGameStateModifier
	{
		public int EntityId { get; }

		public HideEntity(EntityData data)
		{
			EntityId = data.Id;
		}

		public void Apply(IGameState gameState)
		{
			if(gameState.Entities.TryGetValue(EntityId, out var entity))
				entity.Info.Hidden = true;
		}
	}
}
