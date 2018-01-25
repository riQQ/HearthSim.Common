using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.EventManagers;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Test.LogTests.TestHelpers;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogTests
{
	[TestClass]
	public class LogTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var data = TestData.Load("LogTests/TestData/data_1.json");
			var events = new Dictionary<string, object>();
			var gameData = new MockGameData(new MatchInfo()
			{
				LocalPlayer = data.LocalPlayer.ToMatchInfoPlayer(),
				OpposingPlayer = data.OpposingPlayer.ToMatchInfoPlayer()
			});
			var logInput = new MockLogInput();
			var game = new Game(gameData);
			var mgr = new GameEventManager(game, logInput, gameData);

			game.GameCreated += args =>
			{
				events["created"] = true;
				args.Game.LocalPlayer.Deck = new Hearthstone.Deck(HearthDb.Deckstrings.DeckSerializer.Deserialize(data.LocalPlayer.Deck));
			};
			game.GameStateChanged += args =>
			{
				if(args.Modifier is TagChange t)
				{
					if(t.Tag == GameTag.MULLIGAN_STATE && args.State.IsMulliganDone)
					{
						events["t0_local_hand"] = args.State.LocalPlayer.InHand.Select(x => x.CardId).ToList();
						events["t0_opposing_hand"] = args.State.OpposingPlayer.InHand.Select(x => x.CardId).ToList();
					}

					if(t.Tag == GameTag.ZONE && (t.PreviousValue == (int)Zone.DECK || t.Value == (int)Zone.DECK) && t.EntityId.HasValue && args.State.SetupComplete)
					{
						if(args.State.Entities[t.EntityId.Value].IsControlledBy(args.State.LocalPlayer.PlayerId))
							Assert.AreEqual(args.State.LocalPlayer.InDeck.Count(), args.State.LocalPlayer.GetRemainingCards().Sum(x => x.Count));
					}
				}
			};

			//TODO move test data to separate repo

			logInput.Read(data.LogFile);

			Assert.AreEqual(true, events["created"]);

			var localHand = events["t0_local_hand"] as List<string>;
			Assert.IsNotNull(localHand);
			Assert.AreEqual(3, localHand.Count);
			Assert.IsTrue(localHand.All(x => !string.IsNullOrEmpty(x)));

			var opposingHand = events["t0_opposing_hand"] as List<string>;
			Assert.IsNotNull(opposingHand);
			Assert.AreEqual(5, opposingHand.Count);
			Assert.IsTrue(opposingHand.All(string.IsNullOrEmpty));
		}
	}
}
