﻿using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using static HearthDb.CardIds;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	internal class BlockHelper
	{
		private readonly Game _game;

		public BlockHelper(Game game)
		{
			_game = game;
		}

		private string[] Repeat(string str, int count) => Enumerable.Repeat(str, count).ToArray();

		public string[] GetCreatedCards(BlockData block)
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
		private string[] Trigger(BlockData block)
		{
			switch(block.CardId)
			{
				case Rogue.TradePrinceGallywix:
					return new[]
					{
						_game.CurrentGame?.LastCardPlayed?.CardId,
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
					return new[] { block.Target?.CardId };
				default:
					return new string[0];
			}
		}

		/// <summary>
		/// Cards created by Battlecries and spells
		/// </summary>
		public string[] Power(BlockData block)
		{
			switch(block.CardId)
			{
				case Rogue.GangUp:
					return Repeat(block.Target?.CardId, 3);
				case Rogue.BeneathTheGrounds:
					return Repeat(NonCollectible.Rogue.BeneaththeGrounds_NerubianAmbushToken, 3);
				case Warrior.IronJuggernaut:
					return new[] {NonCollectible.Warrior.IronJuggernaut_BurrowingMineToken};
				case Druid.Recycle:
				case Mage.ManicSoulcaster:
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
				default:
					return new string[0];
			}
		}
	}
}
