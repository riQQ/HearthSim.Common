using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class ShowEntity : EntityModifier
	{
		private readonly int _entityId;
		private readonly string _cardId;
		private readonly bool _revealCard;

		public ShowEntity(EntityData data, IBlockData parentBlock) : base(data)
		{
			_entityId = data.Id;
			_cardId = data.CardId;
			_revealCard = parentBlock?.Type == BlockType.REVEAL_CARD;
		}

		public override void Apply2(GameState gameState)
		{
			if(gameState.Entities.TryGetValue(_entityId, out var entity))
			{
				entity.CardId = _cardId;
				entity.Info.Hidden = false;
				entity.Info.JoustReveal = _revealCard;
			}
		}

		public override string ToString()
		{
			return $"SHOW_ENTITY Id={_entityId} CardId={_cardId}";
		}
	}
}
