using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.GameStateModifiers
{
	[TestClass]
	public class ChangeEntityTests
	{
		[TestMethod]
		public void Constructor()
		{
			var mod = new ChangeEntity(new EntityData(1, "NAME", "CARD_ID", Zone.DECK));
			Assert.AreEqual("CARD_ID", mod.CardId);
			Assert.AreEqual(1, mod.EntityId);
		}

		[TestMethod]
		public void Apply_ValidEntity()
		{
			var gs = new MockGameState();
			gs.Entities.Add(1, new Entity(1, "CARD_ID"));
			var mod = new ChangeEntity(new EntityData(1, "NAME", "CARD_ID2", Zone.DECK));
			mod.Apply(gs);
			Assert.AreEqual("CARD_ID2", gs.Entities[1].CardId);
		}

		public void Apply_InvalidEntity()
		{
			var gs = new MockGameState();
			gs.Entities.Add(1, new Entity(1, "CARD_ID"));
			var mod = new ChangeEntity(new EntityData(2, "NAME", "CARD_ID2", Zone.DECK));
			mod.Apply(gs);
			Assert.AreEqual("CARD_ID", gs.Entities[1].CardId);
		}
	}
}
