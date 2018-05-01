using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.EventManagers;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Test.LogTests.TestHelpers;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeckType = HearthSim.Core.Hearthstone.Enums.DeckType;

namespace HearthSim.Core.Test.LogTests
{
	[TestClass]
	public class DungeonRunTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var data = TestData.Load("LogTests/TestData/dungeonrun_1.json");
			var events = new Dictionary<string, object>();
			var gameData = new MockGameData(new MatchInfo()
			{
				LocalPlayer = data.LocalPlayer.ToMatchInfoPlayer(),
				OpposingPlayer = data.OpposingPlayer.ToMatchInfoPlayer()
			}, new GameServerInfo());
			var logInput = new MockLogInput();
			var game = new Game(gameData);
			var mgr = new GameEventManager(game, logInput, gameData);

			var localPlayerDeck = new Hearthstone.Deck(DeckType.DungeonRun, "Dungeon Run", CardClass.PRIEST, data.LocalPlayer.DeckCards);

			game.GameCreated += args => args.Game.LocalPlayer.Deck = localPlayerDeck;

			game.GameStateEvents.TagChange += args =>
			{
				if(args.Data.Tag == GameTag.MULLIGAN_STATE && args.Data.Value == (int) Mulligan.DONE
					&& args.Data.EntityId == args.GameState.LocalPlayerEntity.Id)
				{
					events["t1"] = true;
					Assert.AreEqual(10, args.GameState.LocalPlayer.InDeck.Count());
					Assert.AreEqual(3, args.GameState.LocalPlayer.InHand.Count());
					foreach(var entity in args.GameState.LocalPlayer.InDeck)
						Assert.IsFalse(entity.IsCreated, $"Found created entity in the deck: {entity}");
					foreach(var entity in args.GameState.LocalPlayer.InHand)
						Assert.IsFalse(entity.IsCreated, $"Found created entity in the hand: {entity}");
				}
			};

			logInput.Read(data.LogFile);

			foreach(var turn in new[] {"t1"})
				Assert.AreEqual(true, events[turn]);
		}
	}
}
