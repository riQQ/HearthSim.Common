using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.GameStateModifiers
{
	[TestClass]
	public class EntityModifierTests
	{
		private MockEntityModifier _mod;
		private GameState _gs;

		[TestInitialize]
		public void Initialize()
		{
			_mod = new MockEntityModifier(new EntityData(1, "NAME", "CARD_ID", Zone.DECK));
			_gs = new GameState(new MockGameData());
		}

		[TestMethod]
		public void EntityModfier_ImplementationCalled()
		{
			_mod.Apply(_gs);
			Assert.IsTrue(_mod.Applied);
		}

		[TestMethod]
		public void EntityModfier_CurrentEntitySet()
		{
			_mod.Apply(_gs);
			Assert.AreEqual(1, _gs.CurrentEntity);
		}

		public class MockEntityModifier : EntityModifier
		{
			public MockEntityModifier(EntityData data) : base(data)
			{
			}

			protected override void ApplyImpl(IGameState gameState)
			{
				Applied = true;
			}

			public bool Applied { get; set; }
		}
	}
}
