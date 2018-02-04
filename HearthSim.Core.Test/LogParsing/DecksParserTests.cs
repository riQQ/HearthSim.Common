using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HearthDb.Deckstrings;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class DecksParserTests
	{
		[TestMethod]
		public void EditedDeck()
		{
			Deck deck = null;
			var parser = new DecksParser();
			parser.EditedDeck += args => deck = args.Deck;

			parser.Parse(new Line("Decks", "I 17:18:34.7330418 Finished Editing Deck:"));
			parser.Parse(new Line("Decks", "I 17:18:34.7330418 ### Out of the Woods"));
			parser.Parse(new Line("Decks", "I 17:18:34.7330418 # Deck ID: 1473595360"));
			Assert.IsNull(deck);

			parser.Parse(new Line("Decks", "I 17:18:34.7365509 AAECAZICCIsI4KwCocICh84Cws4CmdMC5+EC+eYCC0BfxAbkCJTSApjSAqjSAovhAoTmAovmAvXnAgA="));
			Assert.IsNotNull(deck);
			Assert.AreEqual("Out of the Woods", deck.Name);
			Assert.AreEqual(1473595360, deck.DeckId);
			Assert.AreEqual(30, deck.CardDbfIds.Sum(x => x.Value));
		}

		[TestMethod]
		public void FoundDecks()
		{
			List<Deck> decks = null;
			var parser = new DecksParser(100);
			parser.FoundDecks += args => decks = args.Decks;

			parser.Parse(new Line("Decks", "I 17:36:16.6978548 Deck Contents Received:"));
			parser.Parse(new Line("Decks", "I 17:18:34.7330418 ### Out of the Woods"));
			parser.Parse(new Line("Decks", "I 17:18:34.7330418 # Deck ID: 1473595360"));
			parser.Parse(new Line("Decks", "I 17:18:34.7365509 AAECAZICCIsI4KwCocICh84Cws4CmdMC5+EC+eYCC0BfxAbkCJTSApjSAqjSAovhAoTmAovmAvXnAgA="));
			parser.Parse(new Line("Decks", "I 17:36:16.7059037 ### Classic Hunter"));
			parser.Parse(new Line("Decks", "I 17:36:16.7059037 # Deck ID: 1473605146"));
			parser.Parse(new Line("Decks", "I 17:36:16.7093861 AAECAYoWBscDyQS7BZkH2Qf4CAyNAdgBqAK1A4cEkgXtBusH2wntCYEK/gwA"));
			Assert.IsNull(decks);

			Thread.Sleep(150);
			Assert.IsNotNull(decks);
			Assert.AreEqual(2, decks.Count);

			Assert.AreEqual("Out of the Woods", decks[0].Name);
			Assert.AreEqual(1473595360, decks[0].DeckId);
			Assert.AreEqual(30, decks[0].CardDbfIds.Sum(x => x.Value));

			Assert.AreEqual("Classic Hunter", decks[1].Name);
			Assert.AreEqual(1473605146, decks[1].DeckId);
			Assert.AreEqual(30, decks[1].CardDbfIds.Sum(x => x.Value));
		}

		[TestMethod]
		public void FindingGame_Deck_Constructed()
		{
			Deck deck = null;
			var heroId = 0;
			var parser = new DecksParser();
			parser.FindingGame += args =>
			{
				deck = args.Deck;
				heroId = args.HeroId;
			};

			parser.Parse(new Line("Decks", "I 17:48:20.5299220 Finding Game With Deck:"));
			parser.Parse(new Line("Decks", "I 17:48:20.5299220 ### Classic Hunter"));
			parser.Parse(new Line("Decks", "I 17:48:20.5299220 # Deck ID: 1473605146"));
			parser.Parse(new Line("Decks", "I 17:48:20.5304233 AAECAYoWBscDyQS7BZkH2Qf4CAyNAdgBqAK1A4cEkgXtBusH2wntCYEK/gwA"));

			Assert.AreEqual("Classic Hunter", deck.Name);
			Assert.AreEqual(1473605146, deck.DeckId);
			Assert.AreEqual(30, deck.CardDbfIds.Sum(x => x.Value));

			Assert.AreEqual(0, heroId);
		}

		[TestMethod]
		public void FindingGame_Deck_Arena()
		{
			Deck deck = null;
			var heroId = 0;
			var parser = new DecksParser();
			parser.FindingGame += args =>
			{
				deck = args.Deck;
				heroId = args.HeroId;
			};

			parser.Parse(new Line("Decks", "I 16:47:47.3726325 Starting Arena Game With Deck:"));
			parser.Parse(new Line("Decks", "I 16:47:47.3726325 ### "));
			parser.Parse(new Line("Decks", "I 16:47:47.3726325 # Deck ID: 1430830822"));
			parser.Parse(new Line("Decks", "I 16:47:47.3726325 AAEBAZ/HAhgI3AGXAtwC2AqarAKGrQLsrgKxrwLqvwLRwQKawgLmwgLKwwLJxwLHywKXzgKZzgK0zgLN0wLS0wLL5gKb5wL86gID0wq/5QLY5QIA"));

			Assert.AreEqual(string.Empty, deck.Name);
			Assert.AreEqual(1430830822, deck.DeckId);
			Assert.AreEqual(30, deck.CardDbfIds.Sum(x => x.Value));

			Assert.AreEqual(0, heroId);
		}

		[TestMethod]
		public void FindingGame_NoDeck()
		{
			Deck deck = null;
			var heroId = 0;
			var parser = new DecksParser();
			parser.FindingGame += args =>
			{
				deck = args.Deck;
				heroId = args.HeroId;
			};

			parser.Parse(new Line("Decks", "I 16:30:07.9804984 Finding Game With Hero: 637"));
			Assert.IsNull(deck);
			Assert.AreEqual(637, heroId);
		}

		[TestMethod]
		public void InvalidLines()
		{
			List <Deck> decks = null;
			var parser = new DecksParser();
			parser.FoundDecks += args => decks = args.Decks;

			parser.Parse(new Line("Decks", "Invalid line 1"));
			parser.Parse(new Line("Decks", "Invalid line 2"));
			parser.Parse(new Line("Decks", "Invalid line 3"));

			Assert.IsNull(decks);
		}

		[TestMethod]
		public void InvalidDeck()
		{
			List <Deck> decks = null;
			var parser = new DecksParser();
			parser.FoundDecks += args => decks = args.Decks;

			parser.Parse(new Line("Decks", "I 16:47:47.3726325 ### "));
			parser.Parse(new Line("Decks", "I 16:47:47.3726325 # Deck ID: 1430830822"));
			parser.Parse(new Line("Decks", "I 16:47:47.3726325 INVALID_DECKSTRING"));

			Assert.IsNull(decks);
		}
	}
}
