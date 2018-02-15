using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.GameStateModifiers
{
	[TestClass]
	public class HideEntityTests
	{
		[TestMethod]
		public void Constructor()
		{
			var entity = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var mod = new HideEntity(entity);
			Assert.AreEqual(entity.Id, mod.EntityId);
		}

		[TestMethod]
		public void Apply_ValidEntity()
		{
			var entity = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var game = new MockGameState();
			game.Entities.Add(1, new Entity(1, "CARD_ID"));
			var mod = new HideEntity(entity);

			mod.Apply(game);
			Assert.IsTrue(game.Entities[entity.Id].Info.Hidden);
		}

		[TestMethod]
		public void Apply_InvalidEntity()
		{
			var entity = new EntityData(2, "NAME", "CARD_ID", Zone.DECK);
			var game = new MockGameState();
			game.Entities.Add(1, new Entity(1, "CARD_ID"));
			var mod = new HideEntity(entity);

			mod.Apply(game);
			Assert.IsFalse(game.Entities[1].Info.Hidden);
		}
	}
}
