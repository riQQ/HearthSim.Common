using System.Collections.Generic;
using HearthDb;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone.Entities
{
	public class Entity
	{
		private Card _card;

		public Entity(int id, string cardId)
		{
			Id = id;
			CardId = cardId;
			Tags = new Dictionary<GameTag, int>();
			Info = new EntityInfo(this);
		}

		public int Id { get; }
		public string CardId { get; set; }
		public Dictionary<GameTag, int> Tags { get; }

		public bool HasCardId => !string.IsNullOrEmpty(CardId);

		public bool IsSecret => HasTag(GameTag.SECRET);

		public bool IsSpell => GetTag(GameTag.CARDTYPE) == (int)CardType.SPELL;

		public bool IsHeroPower => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO_POWER;

		public int Attack => GetTag(GameTag.ATK);

		public int Health => GetTag(GameTag.HEALTH) - GetTag(GameTag.DAMAGE);

		public int Cost => HasTag(GameTag.COST) ? GetTag(GameTag.COST) : Card?.Cost ?? 0;

		public bool IsMinion => GetTag(GameTag.CARDTYPE) == (int)CardType.MINION;

		public bool IsPlayableCard => IsMinion || IsSpell || IsWeapon || IsPlayableHero;

		public bool IsWeapon => GetTag(GameTag.CARDTYPE) == (int)CardType.WEAPON;

		public bool IsInHand => IsInZone(Zone.HAND);

		public bool IsInPlay => IsInZone(Zone.PLAY);

		public bool IsInDeck => IsInZone(Zone.DECK);

		public bool IsInGraveyard => IsInZone(Zone.GRAVEYARD);

		public bool IsInSetAside => IsInZone(Zone.SETASIDE);

		public bool IsInSecret => IsInZone(Zone.SECRET);

		public bool IsQuest => HasTag(GameTag.QUEST);

		public bool IsHero => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO;

		public bool IsPlayableHero => IsHero && Card != null && Card.Set != CardSet.CORE && Card.Set != CardSet.HERO_SKINS && Card.Collectible;

		public bool IsActiveDeathrattle => HasTag(GameTag.DEATHRATTLE) && GetTag(GameTag.DEATHRATTLE) == 1;
		public Card Card => _card ?? (_card = HasCardId && Cards.All.TryGetValue(CardId, out var card) ? card : null);

		public EntityInfo Info { get; }

		public bool IsCreated => HasTag(GameTag.CREATOR) || HasTag(GameTag.DISPLAYED_CREATOR);

		public int GetTag(GameTag tag)
		{
			Tags.TryGetValue(tag, out var value);
			return value;
		}

		public bool HasTag(GameTag tag)
		{
			return GetTag(tag) > 0;
		}

		public bool IsInZone(Zone zone)
		{
			return GetTag(GameTag.ZONE) == (int)zone;
		}

		public bool IsControlledBy(int controllerId)
		{
			return GetTag(GameTag.CONTROLLER) == controllerId;
		}
	}

	public class EntityInfo
	{
		private readonly Entity _entity;
		public EntityInfo(Entity entity)
		{
			_entity = entity;
		}

		//public int Turn { get; set; }
		//public bool Discarded { get; set; }
		//public bool Returned { get; set; }
		//public bool Mulliganed { get; set; }
		public bool Stolen => OriginalController > 0 && OriginalController != _entity.GetTag(GameTag.CONTROLLER);
		//public bool HasOutstandingTagChanges { get; set; }
		public int OriginalController { get; set; }
		public bool Hidden { get; set; }
		//public int CostReduction { get; set; }
		public Zone? OriginalZone { get; set; }
		//public string OriginalCardId { get; private set; }
		//public bool WasTransformed => !string.IsNullOrEmpty(OriginalCardId);
		public bool CreatedInDeck => OriginalZone == Zone.DECK;
		public bool CreatedInHand => OriginalZone == Zone.HAND;
	}
}
