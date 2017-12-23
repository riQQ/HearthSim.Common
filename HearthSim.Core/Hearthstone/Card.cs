﻿using HearthDb;
using HearthDb.Enums;
using System.Linq;
using HearthSim.Core.Util;

namespace HearthSim.Core.Hearthstone
{
	public class Card : ISortableCard
	{
		public static CardSet[] WildSets =
		{
			CardSet.PROMO,
			CardSet.HOF,
			CardSet.NAXX,
			CardSet.GVG,
			CardSet.BRM,
			CardSet.LOE,
			CardSet.TGT
		};

		private HearthDb.Card _card;

		public Card(string id, int count = 1)
		{
			Id = id;
			Count = count;
		}

		public Card(int dbfId, int count = 1) : this(Cards.GetFromDbfId(dbfId, false), count)
		{
		}

		public Card(HearthMirror.Objects.Card card)
		{
			Id = card.Id;
			Count = card.Count;
		}

		public Card(CollectionCard card)
		{
			Id = card.Id;
			Count = card.Normal + card.Golden;
		}

		public Card(HearthDb.Card card, int count = 1)
		{
			Id = card.Id;
			Count = count;
			_card = card;
		}

		public string Id { get; set; }
		public int Count { get; set; }
		public string NamePrefix { get; set; }
		public bool Created { get; set; }

		public HearthDb.Card Data => _card ?? (_card = Cards.All.TryGetValue(Id, out var card) ? card : null);
		public bool IsWild => WildSets.Contains(Data?.Set ?? CardSet.INVALID);

		public int Cost => Data?.Cost ?? 0;

		public string Name => NamePrefix + (Data?.Name ?? string.Empty);

		public Card Clone() => _card != null ? new Card(_card, Count) : new Card(Id, Count);
	}
}
