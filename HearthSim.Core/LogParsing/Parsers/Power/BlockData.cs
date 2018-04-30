using System;
using System.Collections.Generic;
using HearthDb.Enums;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class BlockData : IBlockData
	{
		public BlockData(string type, int id, string cardId, string effectCardId = null, int effectIndex = 0, GameTag? triggerKeyword = 0, EntityData target = null)
		{
			Type = Enum.TryParse(type, out BlockType block) ? (BlockType?)block : null;
			Id = id;
			CardId = cardId;
			EffectCardId = effectCardId;
			EffectIndex = effectIndex;
			TriggerKeyword = triggerKeyword;
			Target = target;
			PredictedCards = new List<string>();
		}

		public EntityData Target { get; }
		public BlockType? Type { get; }
		public int Id { get; }
		public string EffectCardId { get; }
		public int EffectIndex { get; }
		public string CardId { get; }
		public List<string> PredictedCards { get; }
		public GameTag? TriggerKeyword { get; }

		private int _predictedCardIndex;

		public string NextPredictedCard()
			=> PredictedCards.Count > _predictedCardIndex ? PredictedCards[_predictedCardIndex++] : null;

		public void AddPredictedCard(string cardId) => PredictedCards.Add(cardId);
	}
}
