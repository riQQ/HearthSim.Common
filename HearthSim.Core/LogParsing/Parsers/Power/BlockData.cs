﻿using System;
using System.Collections.Generic;
using HearthDb.Enums;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class BlockData
	{
		public BlockData(string type, int id, string cardId, EntityData target = null)
		{
			Type = Enum.TryParse(type, out BlockType block) ? (BlockType?)block : null;
			Id = id;
			CardId = cardId;
			Target = target;
			PredictedCards = new List<string>();
		}

		public EntityData Target { get; }
		public BlockType? Type { get; }
		public int Id { get; }
		public string CardId { get; }
		public List<string> PredictedCards { get; }

		private int _predictedCardIndex;

		public string NextPredictedCard()
			=> PredictedCards.Count > _predictedCardIndex ? PredictedCards[_predictedCardIndex++] : null;
	}
}