using System.Linq;
using HearthDb.Enums;
using static HearthDb.CardIds;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	internal class BlockHelper
	{
		private readonly IGameInfoProvider _gameInfo;

		public BlockHelper(IGameInfoProvider gameInfo)
		{
			_gameInfo = gameInfo;
		}

		private string[] Repeat(string str, int count) => Enumerable.Repeat(str, count).ToArray();

		public string[] GetCreatedCards(IBlockData block)
		{
			switch(block.Type)
			{
				case BlockType.TRIGGER:
					return Trigger(block);
				case BlockType.POWER:
					return Power(block);
				default:
					return new string[0];
			}
		}

		/// <summary>
		/// Cards created by triggers, deathrattles, secrets, etc
		/// </summary>
		private string[] Trigger(IBlockData block)
		{
			switch(block.CardId)
			{
				case Rogue.TradePrinceGallywix:
					return new[]
					{
						_gameInfo.LastPlayedCard,
						NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken
					};
				case Shaman.WhiteEyes:
					return new[] { NonCollectible.Shaman.WhiteEyes_TheStormGuardianToken };
				case Hunter.RaptorHatchling:
					return new[] { NonCollectible.Hunter.RaptorHatchling_RaptorPatriarchToken };
				case Warrior.DirehornHatchling:
					return new[] { NonCollectible.Warrior.DirehornHatchling_DirehornMatriarchToken };
				case Mage.FrozenClone:
					return Repeat(block.Target?.CardId, 2);
				case Shaman.Moorabi:
				case Rogue.SonyaShadowdancer:
					return new[] { block.Target?.CardId };
				case Neutral.HoardingDragon:
					return Repeat(NonCollectible.Neutral.TheCoin, 2);
				case Priest.GildedGargoyle:
					return new[] { NonCollectible.Neutral.TheCoin };
				case Druid.AstralTiger:
					return new[] { Druid.AstralTiger};
				case Rogue.Kingsbane:
					return new[] { Rogue.Kingsbane };
				default:
					return new string[0];
			}
		}

		/// <summary>
		/// Cards created by Battlecries and spells
		/// </summary>
		public string[] Power(IBlockData block)
		{
			switch(block.CardId)
			{
				case Rogue.GangUp:
				case Hunter.DireFrenzy:
					return Repeat(block.Target?.CardId, 3);
				case Rogue.BeneathTheGrounds:
					return Repeat(NonCollectible.Rogue.BeneaththeGrounds_NerubianAmbushToken, 3);
				case Warrior.IronJuggernaut:
					return new[] {NonCollectible.Warrior.IronJuggernaut_BurrowingMineToken};
				case Druid.Recycle:
				case Mage.ManicSoulcaster:
				case Neutral.ZolaTheGorgon:
				case Druid.Splintergraft:
				case Neutral.BalefulBanker:
				case Neutral.DollmasterDorian:
					return new[] {block.Target?.CardId};
				case Mage.ForgottenTorch:
					return new[] {NonCollectible.Mage.ForgottenTorch_RoaringTorchToken};
				case Warlock.CurseOfRafaam:
					return new[] {NonCollectible.Warlock.CurseofRafaam_CursedToken};
				case Neutral.AncientShade:
					return new[] {NonCollectible.Neutral.AncientShade_AncientCurseToken};
				case Priest.ExcavatedEvil:
					return new[] {Priest.ExcavatedEvil};
				case Neutral.EliseStarseeker:
					return new[] {NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken};
				case NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken:
					return new[] {NonCollectible.Neutral.EliseStarseeker_GoldenMonkeyToken};
				case Neutral.Doomcaller:
					return new[] {NonCollectible.Neutral.Cthun};
				case Druid.JadeIdol:
					return Repeat(Druid.JadeIdol, 3);
				case NonCollectible.Hunter.TheMarshQueen_QueenCarnassaToken:
					return Repeat(NonCollectible.Hunter.TheMarshQueen_CarnassasBroodToken, 15);
				case Neutral.EliseTheTrailblazer:
					return new[] {NonCollectible.Neutral.ElisetheTrailblazer_UngoroPackToken};
				case Mage.GhastlyConjurer:
					return new[] {Mage.MirrorImage};
				case Mage.DeckOfWonders:
					return Repeat(NonCollectible.Mage.DeckofWonders_ScrollOfWonderToken, 5);
				case Neutral.TheDarkness:
					return Repeat(NonCollectible.Neutral.TheDarkness_DarknessCandleToken, 3);
				case Rogue.FaldoreiStrider:
					return Repeat(NonCollectible.Rogue.FaldoreiStrider_SpiderAmbushEnchantment, 3);
				case Neutral.KingTogwaggle:
					return new[] { NonCollectible.Neutral.KingTogwaggle_KingsRansomToken };
				case NonCollectible.Neutral.TheCandle:
					return new[] { NonCollectible.Neutral.TheCandle };
				default:
					return new string[0];
			}
		}
	}
}
