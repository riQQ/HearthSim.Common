﻿using System.Collections.Generic;
using HearthDb;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone.Entities
{
	public partial class Entity
	{
		private Card _card;
		private string _cardId;

		public Entity(int id, string cardId)
		{
			Tags = new Dictionary<GameTag, int>();
			Info = new EntityInfo(this);
			Id = id;
			CardId = cardId;
		}

		public int Id { get; }

		public string CardId
		{
			get => _cardId;
			set
			{
				_cardId = value;
				if(string.IsNullOrEmpty(Info.OriginalCardId))
					Info.OriginalCardId = _cardId;
			}
		}

		public Dictionary<GameTag, int> Tags { get; }
		public EntityInfo Info { get; }

		public Card Card => _card ?? (_card = HasCardId && Cards.All.TryGetValue(CardId, out var card) ? new Card(card) : null);

		public bool HasCardId => !string.IsNullOrEmpty(CardId);

		public int GetTag(GameTag tag) => Tags.TryGetValue(tag, out var value) ? value : 0;
		public bool HasTag(GameTag tag) => GetTag(tag) > 0;
		public void SetTag(GameTag tag, int value) => Tags[tag] = value;

		public override string ToString()
		{
			return $"[{Id}{(IsCreated ? "*" : "")}] {Card?.Data?.Name} ({CardId})";
		}
	}

	public class EntityInfo
	{
		private readonly Entity _entity;

		public EntityInfo(Entity entity)
		{
			_entity = entity;
		}

		//public bool Discarded { get; set; }
		//public bool Returned { get; set; }
		//public bool Mulliganed { get; set; }
		public bool Stolen => OriginalController > 0 && OriginalController != _entity.GetTag(GameTag.CONTROLLER);
		//public bool HasOutstandingTagChanges { get; set; }
		public int OriginalController { get; set; }
		public bool Hidden { get; set; }
		//public int CostReduction { get; set; }
		public Zone? OriginalZone { get; set; }
		public string OriginalCardId { get; internal set; }
		//public bool WasTransformed => !string.IsNullOrEmpty(OriginalCardId);
		public bool CreatedInDeck => OriginalZone == Zone.DECK;
		public bool CreatedInHand => OriginalZone == Zone.HAND;
		public bool JoustReveal { get; set; }
		public bool IsCreated { get; set; }
		public int LastZoneChange { get; set; }
		public int PreviousZone { get; set; }
	}
}
