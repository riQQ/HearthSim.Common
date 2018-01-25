using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class BlockHelperTests
	{
		[TestMethod]
		public void TriggerBlock_MissingCardId()
		{
			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.TRIGGER,
				CardId = null
			});
			Assert.IsNotNull(cards);
			Assert.AreEqual(0, cards.Length);
		}

		[TestMethod]
		public void TriggerBlock_LastPlayedCard()
		{
			var blockHelper = new BlockHelper(new MockGameInfo
			{
				LastPlayedCard = Mage.Fireball
			});
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.TRIGGER,
				CardId = Rogue.TradePrinceGallywix
			});
			Assert.AreEqual(2, cards.Length);
			Assert.AreEqual(Mage.Fireball, cards[0]);
			Assert.AreEqual(CardIds.NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken, cards[1]);
		}

		[TestMethod]
		public void TriggerBlock_MultipleTarget()
		{
			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.TRIGGER,
				Target = new EntityData(1, "", Neutral.Alexstrasza, null),
				CardId = Mage.FrozenClone
			});
			Assert.AreEqual(2, cards.Length);
			Assert.AreEqual(Neutral.Alexstrasza, cards[0]);
			Assert.AreEqual(Neutral.Alexstrasza, cards[1]);
		}

		[TestMethod]
		public void TriggerBlock_MissingTarget()
		{
			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.TRIGGER,
				Target = new EntityData(1, "", null, null),
				CardId = Mage.FrozenClone
			});
			Assert.AreEqual(2, cards.Length);
			Assert.AreEqual(null, cards[0]);
			Assert.AreEqual(null, cards[0]);
		}

		[TestMethod]
		public void PowerBlock_MissingCardId()
		{
			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.POWER,
				CardId = null
			});
			Assert.IsNotNull(cards);
			Assert.AreEqual(0, cards.Length);
		}

		[TestMethod]
		public void PowerBlock_Target()
		{
			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.POWER,
				Target = new EntityData(1, "", Neutral.ElvenArcher, null),
				CardId = Mage.ManicSoulcaster
			});
			Assert.AreEqual(1, cards.Length);
			Assert.AreEqual(Neutral.ElvenArcher, cards[0]);
		}

		[TestMethod]
		public void OtherBlock()
		{

			var blockHelper = new BlockHelper(new MockGameInfo());
			var cards = blockHelper.GetCreatedCards(new MockBlockData
			{
				Type = BlockType.DEATHS,
			});
			Assert.IsNotNull(cards);
			Assert.AreEqual(0, cards.Length);
		}
	}
}
