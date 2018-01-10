using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class ShowEntity : EntityModifier
	{
		private readonly int _entityId;
		private readonly string _cardId;

		public ShowEntity(EntityData data) : base(data)
		{
			_entityId = data.Id;
			_cardId = data.CardId;
		}

		public override void Apply2(GameState gameState)
		{
			if(gameState.Entities.TryGetValue(_entityId, out var entity))
			{
				entity.CardId = _cardId;
				entity.Info.Hidden = false;
			}
		}

		public override string ToString()
		{
			return $"SHOW_ENTITY Id={_entityId} CardId={_cardId}";
		}
	}
}
