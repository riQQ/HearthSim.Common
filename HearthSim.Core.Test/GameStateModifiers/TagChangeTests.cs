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
	public class TagChangeTests
	{
		[TestMethod]
		public void Constructor()
		{
			var data = new TagChangeData(GameTag.HEALTH, 5, true, 1, "NAME");
			var tc = new TagChange(data);
			Assert.AreEqual(data.Tag, tc.Tag);
			Assert.AreEqual(data.Value, tc.Value);
			Assert.IsTrue(tc.CreationTag);
			Assert.AreEqual(data.EntityId, tc.EntityId);
			Assert.AreEqual(data.EntityName, tc.EntityName);
		}

		[TestMethod]
		public void CanApply_CreationTag()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, true, null, null));
			Assert.IsTrue(tc.CanApply);
		}

		[TestMethod]
		public void CanApply_HasId()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, 1, null));
			Assert.IsTrue(tc.CanApply);
		}

		[TestMethod]
		public void CanNotApply()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, null, null));
			Assert.IsFalse(tc.CanApply);
		}

		[TestMethod]
		public void ResolveEntity()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, null, null));
			Assert.IsNull(tc.EntityId);
			tc.ResolveEntityId(1);
			Assert.AreEqual(1, tc.EntityId);
		}

		[TestMethod]
		public void ResolveEntity_NoMultipleCalls()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, null, null));
			tc.ResolveEntityId(1);
			tc.ResolveEntityId(2);
			Assert.AreEqual(1, tc.EntityId);
		}

		[TestMethod]
		public void Apply_ValidEntity()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, 1, null));
			var game = new MockGameState();
			game.Entities.Add(1, new Entity(1, "CARD_ID"));

			tc.Apply(game);
			Assert.AreEqual(0, tc.PreviousValue);
			Assert.AreEqual(tc.Value, game.Entities[tc.EntityId.Value].Tags[tc.Tag]);
		}

		[TestMethod]
		public void Apply_ValidEntity_ZoneChange()
		{
			var tc = new TagChange(new TagChangeData(GameTag.ZONE, (int)Zone.DECK, false, 1, null));
			var game = new MockGameState {CurrentTurn = 1};
			game.Entities.Add(1, new Entity(1, "CARD_ID"));

			tc.Apply(game);
			Assert.AreEqual((Zone?)tc.Value, game.Entities[tc.EntityId.Value].Info.OriginalZone);
			Assert.AreEqual(Zone.INVALID, game.Entities[tc.EntityId.Value].Info.PreviousZone);
			Assert.AreEqual(1, game.Entities[tc.EntityId.Value].Info.LastZoneChange);

			var tc2 = new TagChange(new TagChangeData(GameTag.ZONE, (int)Zone.HAND, false, 1, null));
			tc2.Apply(game);
			Assert.AreEqual((Zone?)tc.Value, game.Entities[tc.EntityId.Value].Info.OriginalZone);
		}

		[TestMethod]
		public void Apply_ValidEntity_ControllerChange()
		{
			var tc = new TagChange(new TagChangeData(GameTag.CONTROLLER, 1, false, 1, null));
			var game = new MockGameState();
			game.Entities.Add(1, new Entity(1, "CARD_ID"));

			tc.Apply(game);
			Assert.AreEqual(tc.Value, game.Entities[tc.EntityId.Value].Info.OriginalController);

			var tc2 = new TagChange(new TagChangeData(GameTag.CONTROLLER, 2, false, 1, null));
			tc2.Apply(game);
			Assert.AreEqual(tc.Value, game.Entities[tc.EntityId.Value].Info.OriginalController);
		}

		[TestMethod]
		public void Apply_InvalidEntity()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, 2, null));
			var game = new MockGameState();
			var entity = new Entity(1, "CARD_ID");
			game.Entities.Add(1, entity);

			tc.Apply(game);
			Assert.IsNull(tc.PreviousValue);
		}

		[TestMethod]
		public void Apply_NoEntityId_NoCreationTag()
		{
			var tc = new TagChange(new TagChangeData(GameTag.HEALTH, 5, false, null, null));
			var game = new MockGameState();

			tc.Apply(game);
			Assert.IsNull(tc.EntityId);
			Assert.IsNull(tc.PreviousValue);
		}
	}
}

