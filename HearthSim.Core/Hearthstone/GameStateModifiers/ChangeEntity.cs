using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class ChangeEntity : IGameStateModifier
	{
		public int EntityId { get; }
		public string CardId { get; }

		public ChangeEntity(EntityData data)
		{
			EntityId = data.Id;
			CardId = data.CardId;
		}

		public void Apply(IGameState gameState)
		{
			if(gameState.Entities.TryGetValue(EntityId, out var entity))
				entity.CardId = CardId;
		}

		public override string ToString()
		{
			return $"CHANGE_ENTITY Id={EntityId} CardId={CardId}";
		}
	}
}
