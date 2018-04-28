using System.Collections.Generic;
using HearthDb;
using HearthDb.Enums;
using static HearthDb.CardIds;
using DeckType = HearthSim.Core.Hearthstone.Enums.DeckType;

namespace HearthSim.Core.Hearthstone
{
	public class DungeonRun
	{
		public static Deck GetDefaultDeck(CardClass cardClass, CardSet set)
		{
			var decks = GetClassDecks(set);
			if(decks == null || !decks.TryGetValue(cardClass, out var cards))
				return null;
			return new Deck(DeckType.DungeonRun, "Dungeon Run", cardClass, cards);
		}

		private static readonly List<CardSet> DungeonRunSets = new List<CardSet> { CardSet.LOOTAPALOOZA, CardSet.GILNEAS };

		public static bool IsDungeonBoss(string cardId) =>
			cardId != null && Cards.All.TryGetValue(cardId, out var card) && IsDungeonBoss(card);

		public static bool IsDungeonBoss(HearthDb.Card card) =>
			card != null && card.Id.Contains("BOSS") && DungeonRunSets.Contains(card.Set);

		private static Dictionary<CardClass, List<string>> GetClassDecks(CardSet set)
		{
			switch(set)
			{
				case CardSet.LOOTAPALOOZA:
					return DefaultLootDecks;
				case CardSet.GILNEAS:
					return DefaultGilneasDecks;
			}
			return null;
		}

		private static readonly Dictionary<CardClass, List<string>> DefaultGilneasDecks = new Dictionary<CardClass, List<string>>
		{
			[CardClass.ROGUE] = new List<string>
			{
				Collectible.Neutral.ElvenArcher,
				Collectible.Rogue.SinisterStrike,
				Collectible.Neutral.WorgenInfiltrator,
				Collectible.Neutral.BloodsailRaider,
				Collectible.Hunter.Glaivezooka,
				Collectible.Hunter.SnakeTrap,
				Collectible.Rogue.BlinkFox,
				Collectible.Rogue.FanOfKnives,
				Collectible.Neutral.HiredGun,
				Collectible.Rogue.Si7Agent
			},
			[CardClass.WARRIOR] = new List<string>
			{
				Collectible.Neutral.AbusiveSergeant,
				NonCollectible.Neutral.ExtraPowder,
				Collectible.Neutral.LowlySquire,
				Collectible.Neutral.AmaniBerserker,
				Collectible.Warrior.CruelTaskmaster,
				Collectible.Warrior.RedbandWasp,
				Collectible.Warrior.Bash,
				Collectible.Warrior.FierceMonkey,
				Collectible.Warrior.KingsDefender,
				Collectible.Warrior.BloodhoofBrave
			},
			[CardClass.HUNTER] = new List<string>
			{
				Collectible.Hunter.FieryBat,
				Collectible.Hunter.OnTheHunt,
				Collectible.Neutral.SwampLeech,
				Collectible.Hunter.CracklingRazormaw,
				Collectible.Hunter.HuntingMastiff,
				Collectible.Hunter.ForlornStalker,
				Collectible.Hunter.KillCommand,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Hunter.Houndmaster,
				Collectible.Neutral.SwiftMessenger
			},
			[CardClass.MAGE] = new List<string>
			{
				Collectible.Mage.ArcaneMissiles,
				Collectible.Mage.ManaWyrm,
				Collectible.Neutral.MadBomber,
				Collectible.Mage.PrimordialGlyph,
				Collectible.Mage.ShimmeringTempest,
				Collectible.Mage.UnstablePortal,
				Collectible.Mage.Spellslinger,
				Collectible.Neutral.TinkmasterOverspark,
				Collectible.Mage.WaterElemental,
				Collectible.Neutral.Blingtron3000
			},
		};

		private static readonly Dictionary<CardClass, List<string>> DefaultLootDecks = new Dictionary<CardClass, List<string>>
		{
			[CardClass.ROGUE] = new List<string>
			{
				Collectible.Rogue.Backstab,
				Collectible.Rogue.DeadlyPoison,
				Collectible.Rogue.PitSnake,
				Collectible.Rogue.SinisterStrike,
				Collectible.Neutral.GilblinStalker,
				Collectible.Rogue.UndercityHuckster,
				Collectible.Rogue.Si7Agent,
				Collectible.Rogue.UnearthedRaptor,
				Collectible.Rogue.Assassinate,
				Collectible.Rogue.Vanish,
			},
			[CardClass.WARRIOR] = new List<string>
			{
				Collectible.Warrior.Warbot,
				Collectible.Neutral.AmaniBerserker,
				Collectible.Warrior.CruelTaskmaster,
				Collectible.Warrior.HeroicStrike,
				Collectible.Warrior.Bash,
				Collectible.Warrior.FieryWarAxe,
				Collectible.Neutral.HiredGun,
				Collectible.Neutral.RagingWorgen,
				Collectible.Neutral.DreadCorsair,
				Collectible.Warrior.Brawl,
			},
			[CardClass.SHAMAN] = new List<string>
			{
				Collectible.Shaman.AirElemental,
				Collectible.Shaman.LightningBolt,
				Collectible.Shaman.FlametongueTotem,
				Collectible.Neutral.MurlocTidehunter,
				Collectible.Shaman.StormforgedAxe,
				Collectible.Shaman.LightningStorm,
				Collectible.Shaman.UnboundElemental,
				Collectible.Neutral.DefenderOfArgus,
				Collectible.Shaman.Hex,
				Collectible.Shaman.FireElemental,
			},
			[CardClass.PALADIN] = new List<string>
			{
				Collectible.Paladin.BlessingOfMight,
				Collectible.Neutral.GoldshireFootman,
				Collectible.Paladin.NobleSacrifice,
				Collectible.Paladin.ArgentProtector,
				Collectible.Paladin.Equality,
				Collectible.Paladin.HolyLight,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Paladin.Consecration,
				Collectible.Neutral.StormwindKnight,
				Collectible.Paladin.TruesilverChampion,
			},
			[CardClass.HUNTER] = new List<string>
			{
				Collectible.Hunter.HuntersMark,
				Collectible.Neutral.StonetuskBoar,
				Collectible.Neutral.DireWolfAlpha,
				Collectible.Hunter.ExplosiveTrap,
				Collectible.Hunter.AnimalCompanion,
				Collectible.Hunter.DeadlyShot,
				Collectible.Hunter.EaglehornBow,
				Collectible.Neutral.JunglePanther,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Neutral.OasisSnapjaw,
			},
			[CardClass.DRUID] = new List<string>
			{
				Collectible.Druid.EnchantedRaven,
				Collectible.Druid.PowerOfTheWild,
				Collectible.Druid.TortollanForager,
				Collectible.Druid.MountedRaptor,
				Collectible.Druid.Mulch,
				Collectible.Neutral.ShadeOfNaxxramas,
				Collectible.Druid.KeeperOfTheGrove,
				Collectible.Druid.SavageCombatant,
				Collectible.Druid.Swipe,
				Collectible.Druid.DruidOfTheClaw,
			},
			[CardClass.WARLOCK] = new List<string>
			{
				Collectible.Warlock.Corruption,
				Collectible.Warlock.MortalCoil,
				Collectible.Warlock.Voidwalker,
				Collectible.Neutral.KnifeJuggler,
				Collectible.Neutral.SunfuryProtector,
				Collectible.Warlock.DrainLife,
				Collectible.Neutral.ImpMaster,
				Collectible.Neutral.DarkIronDwarf,
				Collectible.Warlock.Hellfire,
				Collectible.Warlock.Doomguard,
			},
			[CardClass.MAGE] = new List<string>
			{
				Collectible.Mage.ArcaneMissiles,
				Collectible.Mage.ManaWyrm,
				Collectible.Neutral.Doomsayer,
				Collectible.Mage.Frostbolt,
				Collectible.Mage.SorcerersApprentice,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Mage.IceBarrier,
				Collectible.Neutral.ChillwindYeti,
				Collectible.Mage.Fireball,
				Collectible.Mage.Blizzard,
			},
			[CardClass.PRIEST] = new List<string>
			{
				Collectible.Priest.HolySmite,
				Collectible.Priest.NorthshireCleric,
				Collectible.Priest.PotionOfMadness,
				Collectible.Neutral.FaerieDragon,
				Collectible.Priest.MindBlast,
				Collectible.Priest.ShadowWordPain,
				Collectible.Priest.DarkCultist,
				Collectible.Priest.AuchenaiSoulpriest,
				Collectible.Priest.Lightspawn,
				Collectible.Priest.HolyNova,
			}
		};
	}
}
