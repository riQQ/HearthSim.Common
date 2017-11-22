﻿using HearthSim.Core.LogParsing.Parsers.Power;

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

		public override void Apply2(GameState gameState)
		{
			gameState.Entities[_entityId].CardId = _cardId;
		}

		public override string ToString()
		{
			return $"CHANGE_ENTITY Id={_entityId} CardId={_cardId}";
		}
	}
}
