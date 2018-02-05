using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.GameStateModifiers
{
	[TestClass]
	public class ShowEntityTests
	{
		[TestMethod]
		public void Constructor()
		{
			var entity = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var block = new MockBlockData();
			var mod = new ShowEntity(entity, block);
			Assert.AreEqual(entity.Id, mod.EntityId);
			Assert.AreEqual(entity.CardId, mod.CardId);
			Assert.AreEqual(block, mod.ParentBlock);
		}

		[TestMethod]
		public void Apply_ValidEntity()
		{
			var entityData = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var game = new GameState(new MockGameData());
			var entity = new Entity(1, "");
			entity.Info.Hidden = true;
			game.Entities.Add(1, entity);
			var mod = new ShowEntity(entityData, new MockBlockData());

			mod.Apply(game);
			Assert.IsFalse(game.Entities[entityData.Id].Info.Hidden);
			Assert.IsFalse(game.Entities[entityData.Id].Info.JoustReveal);
			Assert.AreEqual(entityData.CardId, game.Entities[entityData.Id].CardId);
		}

		[TestMethod]
		public void Apply_ValidEntity_RevealCard()
		{
			var entity = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var game = new GameState(new MockGameData());
			game.Entities.Add(1, new Entity(1, ""));
			var mod = new ShowEntity(entity, new MockBlockData {Type = BlockType.REVEAL_CARD});

			mod.Apply(game);
			Assert.IsTrue(game.Entities[entity.Id].Info.JoustReveal);
		}

		[TestMethod]
		public void Apply_InvalidEntity()
		{
			var entityData = new EntityData(2, "NAME", "CARD_ID", Zone.DECK);
			var game = new GameState(new MockGameData());
			var entity = new Entity(1, "");
			entity.Info.Hidden = true;
			game.Entities.Add(1, entity);
			var mod = new ShowEntity(entityData, new MockBlockData());

			mod.Apply(game);
			Assert.AreNotEqual(entityData.CardId, game.Entities[1].CardId);
			Assert.IsTrue(game.Entities[1].Info.Hidden);
			Assert.AreEqual(1, game.Entities.Count);
		}
	}
}
