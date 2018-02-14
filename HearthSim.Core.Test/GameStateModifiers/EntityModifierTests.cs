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
		private MockGameState _gs;

		[TestInitialize]
		public void Initialize()
		{
			_mod = new MockEntityModifier(new EntityData(1, "NAME", "CARD_ID", Zone.DECK));
			_gs = new MockGameState();
		}

		[TestMethod]
		public void EntityModfier_ImplementationCalled()
		{
			_mod.Apply(_gs);
			Assert.IsTrue(_mod.Applied);
		}

		public class MockEntityModifier : IGameStateModifier
		{
			public MockEntityModifier(EntityData data)
			{
			}

			public void Apply(IGameState gameState)
			{
				Applied = true;
			}

			public bool Applied { get; set; }
		}
	}
}
