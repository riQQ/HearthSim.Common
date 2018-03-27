using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone.Entities
{
	public partial class Entity
	{
		// Stats
		public int Attack => GetTag(GameTag.ATK);
		public int Health => GetTag(GameTag.HEALTH) - GetTag(GameTag.DAMAGE);
		public int Cost => HasTag(GameTag.COST) ? GetTag(GameTag.COST) : Card?.Data?.Cost ?? 0;
		public bool IsActiveDeathrattle => GetTag(GameTag.DEATHRATTLE) == 1;
		public CardClass Class => (CardClass)GetTag(GameTag.CLASS);

		// Zone
		public bool IsInZone(Zone zone) => GetTag(GameTag.ZONE) == (int)zone;
		public bool IsInHand => IsInZone(Zone.HAND);
		public bool IsInPlay => IsInZone(Zone.PLAY);
		public bool IsInDeck => IsInZone(Zone.DECK);
		public bool IsInGraveyard => IsInZone(Zone.GRAVEYARD);
		public bool IsInSetAside => IsInZone(Zone.SETASIDE);
		public bool IsInSecret => IsInZone(Zone.SECRET);

		// Card Type
		public bool IsQuest => HasTag(GameTag.QUEST);
		public bool IsHero => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO;
		public bool IsMinion => GetTag(GameTag.CARDTYPE) == (int)CardType.MINION;
		public bool IsWeapon => GetTag(GameTag.CARDTYPE) == (int)CardType.WEAPON;
		public bool IsSecret => HasTag(GameTag.SECRET);
		public bool IsSpell => GetTag(GameTag.CARDTYPE) == (int)CardType.SPELL;
		public bool IsHeroPower => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO_POWER;
		public bool IsPlayableHero => IsHero && Card?.Data != null && Card.Data.Set != CardSet.CORE && Card.Data.Set != CardSet.HERO_SKINS && Card.Data.Collectible;
		public bool IsPlayableCard => IsMinion || IsSpell || IsWeapon || IsPlayableHero;

		// Other
		public bool IsCreated => HasTag(GameTag.CREATOR) || Info.JoustReveal || Info.IsCreated;
		public bool IsControlledBy(int controllerId) => GetTag(GameTag.CONTROLLER) == controllerId;
	}
}
