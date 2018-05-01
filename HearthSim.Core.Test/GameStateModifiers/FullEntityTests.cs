using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.GameStateModifiers
{
	[TestClass]
	public class FullEntityTests
	{
		private MockGameState _game;

		[TestInitialize]
		public void Initialize()
		{
			_game = new MockGameState();
		}

		[TestMethod]
		public void Constructor()
		{
			var entity = new EntityData(1, "NAME", "CARD_ID", Zone.DECK);
			var block = new MockBlockData();
			var mod = new FullEntity(entity, block);
			Assert.AreEqual(entity, mod.Data);
			Assert.AreEqual(block, mod.ParentBlock);
		}

		[TestMethod]
		public void Apply_GameEntity()
		{
			var entity = new GameEntityData(1);
			var mod = new FullEntity(entity, new MockBlockData());
			mod.Apply(_game);
			Assert.AreEqual(1, _game.Entities.Count);
			Assert.IsNotNull(_game.GameEntity);
			Assert.IsNotNull(_game.Entities[entity.Id]);
			Assert.AreEqual(entity.Id, _game.GameEntity.Id);
			Assert.AreEqual(entity.Id, _game.Entities[entity.Id].Id);
		}

		[TestMethod]
		public void Apply_PlayerEntity()
		{
			var p1 = new PlayerEntityData(2, 1);
			var p2 = new PlayerEntityData(3, 2);
			var mod1 = new FullEntity(p1, new MockBlockData());
			var mod2 = new FullEntity(p2, new MockBlockData());
			mod1.Apply(_game);
			mod2.Apply(_game);
			Assert.AreEqual(2, _game.Entities.Count);
			Assert.IsNotNull(_game.Entities[p1.Id] as PlayerEntity);
			Assert.IsNotNull(_game.PlayerEntities[p1.PlayerId]);
			Assert.AreEqual(p1.PlayerId, _game.PlayerEntities[p1.PlayerId].PlayerId);
			Assert.IsNotNull(_game.Entities[p2.Id] as PlayerEntity);
			Assert.IsNotNull(_game.PlayerEntities[p2.PlayerId]);
			Assert.AreEqual(p2.PlayerId, _game.PlayerEntities[p2.PlayerId].PlayerId);
		}

		[TestMethod]
		public void Apply_Entity()
		{
			var data = new EntityData(1, "NAME", "CARD_ID", Zone.SETASIDE);
			var mod = new FullEntity(data, new MockBlockData());

			mod.Apply(_game);
			var entity = _game.Entities[data.Id];
			Assert.IsNotNull(entity);
			Assert.AreEqual(data.Id, entity.Id);
			Assert.AreEqual(data.CardId, entity.CardId);
			Assert.IsTrue(entity.IsCreated);
			Assert.AreEqual((int)data.Zone, entity.GetTag(GameTag.ZONE));
			Assert.IsFalse(entity.Info.JoustReveal);
			Assert.AreEqual(data.Zone, entity.Info.OriginalZone);
		}

		[TestMethod]
		public void Apply_Entity_MulliganDone()
		{
			var game = new MockGameState {IsMulliganDone = true, SetupComplete = true};
			var data = new EntityData(1, "NAME", "CARD_ID", Zone.SETASIDE);
			var mod = new FullEntity(data, new MockBlockData());

			mod.Apply(game);
			var entity = game.Entities[data.Id];
			Assert.IsTrue(entity.IsCreated);
		}

		[TestMethod]
		public void Apply_Entity_JoustReveal()
		{
			var data = new EntityData(1, "NAME", "CARD_ID", Zone.SETASIDE);
			var mod = new FullEntity(data, new MockBlockData {Type = BlockType.JOUST});

			mod.Apply(_game);
			var entity = _game.Entities[data.Id];
			Assert.IsTrue(entity.Info.JoustReveal);
		}
	}
}
