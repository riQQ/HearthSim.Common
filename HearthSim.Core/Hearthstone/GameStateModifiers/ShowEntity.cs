using HearthSim.Core.LogParsing.EventArgs;
using HearthSim.Core.LogParsing.Parsers.PowerData;

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
			gameState.Entities[_entityId].CardId = _cardId;
		}

		public override string ToString()
		{
			return $"SHOW_ENTITY Id={_entityId} CardId={_cardId}";
		}
	}
}
