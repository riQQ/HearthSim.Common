using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class HideEntity : EntityModifier
	{
		public int EntityId { get; }

		public HideEntity(EntityData data) : base(data)
		{
			EntityId = data.Id;
		}

		protected override void ApplyImpl(GameState gameState)
		{
			if(gameState.Entities.TryGetValue(EntityId, out var entity))
				entity.Info.Hidden = true;
		}
	}
}
