using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class ChangeEntity : EntityModifier
	{
		private readonly int _entityId;
		private readonly string _cardId;

		public ChangeEntity(EntityData data) : base(data)
		{
			_entityId = data.Id;
			_cardId = data.CardId;
		}

		protected override void ApplyImpl(GameState gameState)
		{
			if(gameState.Entities.TryGetValue(_entityId, out var entity))
				entity.CardId = _cardId;
		}

		public override string ToString()
		{
			return $"CHANGE_ENTITY Id={_entityId} CardId={_cardId}";
		}
	}
}
