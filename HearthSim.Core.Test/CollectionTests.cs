using System.Collections.Generic;
using System.Linq;
using HearthDb.Deckstrings;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util.EventArgs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Deck = HearthDb.Deckstrings.Deck;

namespace HearthSim.Core.Test
{
	[TestClass]
	public class CollectionTests
	{
		private const int BaseFavoriteCardBack = 5;
		private const int BaseDust = 100;
		private const int BaseGold = 500;
		private Collection _collection;

		private List<CollectionCard> BaseCards => new List<CollectionCard>
		{
			new CollectionCard("ID_1", 1, 0),
			new CollectionCard("ID_2", 0, 1),
			new CollectionCard("ID_3", 2, 0),
			new CollectionCard("ID_4", 1, 1),
			new CollectionCard("ID_5", 0, 2),
			new CollectionCard("ID_6", 2, 2)
		};

		private List<int> BaseCardBacks => new List<int>
		{
			1,
			2,
			3
		};

		private List<CollectionCard> BaseHeroes => new List<CollectionCard>
		{
			new CollectionCard("ID_1", 1, 0),
			new CollectionCard("ID_2", 0, 1)
		};

		[TestInitialize]
		public void Initialize()
		{
			_collection = new Collection();
		}

		public void UpdateCollection(IList<CollectionCard> cards = null, IList<int> cardBacks = null,
			IList<CollectionCard> favoriteHeroes = null, int? favoriteCardBack = null,
			int? dust = null, int? gold = null)
		{
			_collection.Update(cards ?? BaseCards, cardBacks ?? BaseCardBacks, favoriteHeroes ?? BaseHeroes,
				favoriteCardBack ?? BaseFavoriteCardBack, dust ?? BaseDust, gold ?? BaseGold);
		}

		[TestMethod]
		public void Initial_CardsLoaded()
		{
			List<CollectionCard> cards = null;
			List<CollectionCard> added = null;
			List<CollectionCard> removed = null;
			_collection.CardsChanged += args =>
			{
				cards = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};
			UpdateCollection();
			Assert.IsNotNull(cards);
			var baseCards = BaseCards;
			Assert.IsTrue(cards.All(x =>
				baseCards.Any(x2 => x.Id == x2.Id
									&& x.Normal == x2.Normal
									&& x.Golden == x2.Golden
				)));
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
		}

		[TestMethod]
		public void Update_CardsChanged()
		{
			UpdateCollection();
			List<CollectionCard> cards = null;
			List<CollectionCard> added = null;
			List<CollectionCard> removed = null;
			_collection.CardsChanged += args =>
			{
				cards = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};
			var newCards = new List<CollectionCard>
			{
				new CollectionCard("ID_1", 2, 0),
				new CollectionCard("ID_2", 0, 3),
				new CollectionCard("ID_3", 3, 1),
				new CollectionCard("ID_5", 0, 1),
				new CollectionCard("ID_6", 1, 1),
				new CollectionCard("ID_7", 1, 1)
			};
			UpdateCollection(newCards);
			Assert.IsNotNull(cards);
			Assert.IsTrue(cards.All(x =>
				newCards.Any(x2 => x.Id == x2.Id
									&& x.Normal == x2.Normal
									&& x.Golden == x2.Golden
				)));
			Assert.IsNotNull(added);
			Assert.AreEqual(4, added.Count);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_1").Normal);
			Assert.AreEqual(0, added.Single(x => x.Id == "ID_1").Golden);
			Assert.AreEqual(0, added.Single(x => x.Id == "ID_2").Normal);
			Assert.AreEqual(2, added.Single(x => x.Id == "ID_2").Golden);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_3").Normal);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_3").Golden);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_7").Normal);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_7").Golden);
			Assert.IsNotNull(removed);
			Assert.AreEqual(3, removed.Count);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_4").Normal);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_4").Golden);
			Assert.AreEqual(0, removed.Single(x => x.Id == "ID_5").Normal);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_5").Golden);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_6").Normal);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_6").Golden);
		}

		[TestMethod]
		public void Initial_CardBacksLoaded()
		{
			List<int> cardBacks = null;
			List<int> added = null;
			List<int> removed = null;
			_collection.CardBacksChanged += args =>
			{
				cardBacks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};
			UpdateCollection();
			Assert.IsNotNull(cardBacks);
			Assert.IsTrue(cardBacks.SequenceEqual(BaseCardBacks));
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
		}

		[TestMethod]
		public void Update_CardBacksChanged()
		{
			UpdateCollection();
			List<int> cardBacks = null;
			List<int> added = null;
			List<int> removed = null;
			_collection.CardBacksChanged += args =>
			{
				cardBacks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};
			var newCardBacks = new List<int>
			{
				1,
				2,
				4
			};
			UpdateCollection(cardBacks: newCardBacks);
			Assert.IsNotNull(cardBacks);
			Assert.IsTrue(cardBacks.SequenceEqual(newCardBacks));
			Assert.IsNotNull(added);
			Assert.AreEqual(1, added.Count);
			Assert.AreEqual(4, added.Single());
			Assert.IsNotNull(removed);
			Assert.AreEqual(1, removed.Count);
			Assert.AreEqual(3, removed.Single());
		}

		[TestMethod]
		public void Initial_FavoriteHeroesLoaded()
		{
			List<CollectionCard> heroes = null;
			List<CollectionCard> added = null;
			List<CollectionCard> removed = null;
			_collection.FavoriteHeroesChanged += args =>
			{
				heroes = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};
			UpdateCollection();
			Assert.IsNotNull(heroes);
			var baseHeroes = BaseHeroes;
			Assert.IsTrue(heroes.All(x =>
				baseHeroes.Any(x2 => x.Id == x2.Id
									&& x.Normal == x2.Normal
									&& x.Golden == x2.Golden
				)));
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
		}

		[TestMethod]
		public void Update_FavoriteHeroesChanged()
		{
			UpdateCollection();
			List<CollectionCard> heroes = null;
			List<CollectionCard> added = null;
			List<CollectionCard> removed = null;
			_collection.FavoriteHeroesChanged += args =>
			{
				heroes = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
			};

			var newHeroes = new List<CollectionCard>
			{
				new CollectionCard("ID_1", 0, 1),
				new CollectionCard("ID_3", 0, 1)
			};
			UpdateCollection(favoriteHeroes: newHeroes);
			Assert.IsNotNull(heroes);
			Assert.IsTrue(heroes.All(x =>
				newHeroes.Any(x2 => x.Id == x2.Id
									&& x.Normal == x2.Normal
									&& x.Golden == x2.Golden
				)));
			Assert.IsNotNull(added);
			Assert.AreEqual(2, added.Count);
			Assert.AreEqual(0, added.Single(x => x.Id == "ID_1").Normal);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_1").Golden);
			Assert.AreEqual(0, added.Single(x => x.Id == "ID_3").Normal);
			Assert.AreEqual(1, added.Single(x => x.Id == "ID_3").Golden);
			Assert.IsNotNull(removed);
			Assert.AreEqual(2, removed.Count);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_1").Normal);
			Assert.AreEqual(0, removed.Single(x => x.Id == "ID_1").Golden);
			Assert.AreEqual(0, removed.Single(x => x.Id == "ID_2").Normal);
			Assert.AreEqual(1, removed.Single(x => x.Id == "ID_2").Golden);
		}

		[TestMethod]
		public void Initial_FavoriteCardBackLoaded()
		{
			int? cardBack = null;
			_collection.FavoriteCardBackChanged += args => { cardBack = args.NewValue; };
			UpdateCollection();
			Assert.IsNotNull(cardBack);
			Assert.AreEqual(BaseFavoriteCardBack, cardBack);
		}

		[TestMethod]
		public void Update_FavoriteCardBackChanged()
		{
			UpdateCollection();
			int? cardBack = null;
			int? prevCardBack = null;
			_collection.FavoriteCardBackChanged += args =>
			{
				cardBack = args.NewValue;
				prevCardBack = args.OldValue;
			};
			UpdateCollection(favoriteCardBack: 10);
			Assert.IsNotNull(cardBack);
			Assert.AreEqual(10, cardBack);
			Assert.IsNotNull(prevCardBack);
			Assert.AreEqual(BaseFavoriteCardBack, prevCardBack);
		}

		[TestMethod]
		public void Initial_DustLoaded()
		{
			int? dust = null;
			_collection.DustChanged += args => { dust = args.NewValue; };
			UpdateCollection();
			Assert.IsNotNull(dust);
			Assert.AreEqual(BaseDust, dust);
		}

		[TestMethod]
		public void Update_DustChanged()
		{
			UpdateCollection();
			int? dust = null;
			int? prevDust = null;
			_collection.DustChanged += args =>
			{
				dust = args.NewValue;
				prevDust = args.OldValue;
			};
			UpdateCollection(dust: 200);
			Assert.IsNotNull(dust);
			Assert.AreEqual(200, dust);
			Assert.IsNotNull(prevDust);
			Assert.AreEqual(BaseDust, prevDust);
		}

		[TestMethod]
		public void Initial_GoldLoaded()
		{
			int? gold = null;
			_collection.GoldChanged += args => { gold = args.NewValue; };
			UpdateCollection();
			Assert.IsNotNull(gold);
			Assert.AreEqual(BaseGold, gold);
		}

		[TestMethod]
		public void Update_GoldChanged()
		{
			UpdateCollection();
			int? gold = null;
			int? prevGold = null;
			_collection.GoldChanged += args =>
			{
				gold = args.NewValue;
				prevGold = args.OldValue;
			};
			UpdateCollection(gold: 1000);
			Assert.IsNotNull(gold);
			Assert.AreEqual(1000, gold);
			Assert.IsNotNull(prevGold);
			Assert.AreEqual(BaseGold, prevGold);
		}

		private void LoadDecks()
		{
			var deck1 = DeckSerializer.Deserialize("AAECAZ8FBvQFucECg8cC1uUCteYCt+cCDKcF9QXZB7EI2a4Cu68C/68CuMcC2ccC48sC+NICieYCAA==");
			deck1.DeckId = 1;
			var deck2 = DeckSerializer.Deserialize("AAECAf0GBOCsApfTAp3iAtvpAg2KAZME9wS2B+EH3sQC58sC8tAC+NACiNICi+EC/OUC6OcCAA==");
			deck2.DeckId = 2;
			_collection.OnDecksLoaded(new ConstructedDeckFoundEventArgs(new List<Deck>
			{
				deck1,
				deck2
			}));
		}

		[TestMethod]
		public void DecksLoaded()
		{
			List<Hearthstone.Deck> decks = null;
			List<Hearthstone.Deck> added = null;
			List<Hearthstone.Deck> removed = null;
			List<Hearthstone.Deck> edited = null;
			_collection.DecksChanged += args =>
			{
				decks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
				edited = args.Edited.ToList();
			};
			LoadDecks();
			Assert.IsNotNull(decks);
			Assert.AreEqual(2, decks.Count);
			Assert.AreEqual(30, decks.Single(x => x.DeckId == 1).Cards.Sum(x => x.Count));
			Assert.AreEqual(30, decks.Single(x => x.DeckId == 2).Cards.Sum(x => x.Count));
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
			Assert.IsNotNull(edited);
			Assert.AreEqual(0, edited.Count);
		}

		[TestMethod]
		public void DecksEdited_ExistingDeck()
		{
			List<Hearthstone.Deck> decks = null;
			List<Hearthstone.Deck> added = null;
			List<Hearthstone.Deck> removed = null;
			List<Hearthstone.Deck> edited = null;
			LoadDecks();
			Assert.AreEqual(2, _collection.Decks.Count());
			_collection.DecksChanged += args =>
			{
				decks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
				edited = args.Edited.ToList();
			};
			var deck = DeckSerializer.Deserialize("AAECAZ8FBvQF2a4CucECg8cCteYCt+cCDKcF9QXZB7EIu68C/68CuMcC2ccC48sC+NIC1uUCieYCAA==");
			deck.DeckId = 1;
			deck.Name = "existing deck";
			_collection.OnDeckEdited(new DeckEditedEventArgs(deck));
			Assert.IsNotNull(decks);
			Assert.AreEqual(2, decks.Count);
			Assert.AreEqual(deck.Name, decks.Single(x => x.DeckId == 1 ).Name);
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
			Assert.IsNotNull(edited);
			Assert.AreEqual(1, edited.Count);
			Assert.AreEqual(deck.Name, edited.Single().Name);
		}

		[TestMethod]
		public void DecksEdited_NewDeck()
		{
			List<Hearthstone.Deck> decks = null;
			List<Hearthstone.Deck> added = null;
			List<Hearthstone.Deck> removed = null;
			List<Hearthstone.Deck> edited = null;
			LoadDecks();
			Assert.AreEqual(2, _collection.Decks.Count());
			_collection.DecksChanged += args =>
			{
				decks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
				edited = args.Edited.ToList();
			};
			var deck = DeckSerializer.Deserialize("AAECAZ8FBvQF2a4CucECg8cCteYCt+cCDKcF9QXZB7EIu68C/68CuMcC2ccC48sC+NIC1uUCieYCAA==");
			deck.DeckId = 3;
			deck.Name = "new deck";
			_collection.OnDeckEdited(new DeckEditedEventArgs(deck));
			Assert.IsNotNull(decks);
			Assert.AreEqual(3, decks.Count);
			Assert.AreEqual(deck.Name, decks.Single(x => x.DeckId == 3).Name);
			Assert.IsNotNull(added);
			Assert.AreEqual(1, added.Count);
			Assert.AreEqual(deck.Name, added.Single().Name);
			Assert.IsNotNull(removed);
			Assert.AreEqual(0, removed.Count);
			Assert.IsNotNull(edited);
			Assert.AreEqual(0, edited.Count);
		}

		[TestMethod]
		public void DeckDeleted()
		{
			List<Hearthstone.Deck> decks = null;
			List<Hearthstone.Deck> added = null;
			List<Hearthstone.Deck> removed = null;
			List<Hearthstone.Deck> edited = null;
			LoadDecks();
			Assert.AreEqual(2, _collection.Decks.Count());
			_collection.DecksChanged += args =>
			{
				decks = args.Items.ToList();
				added = args.Added.ToList();
				removed = args.Removed.ToList();
				edited = args.Edited.ToList();
			};
			_collection.OnDeckDeleted(new DeckDeletedEventArgs(1));
			Assert.IsNotNull(decks);
			Assert.AreEqual(1, decks.Count);
			Assert.AreEqual(2, decks.Single().DeckId);
			Assert.IsNotNull(added);
			Assert.AreEqual(0, added.Count);
			Assert.IsNotNull(removed);
			Assert.AreEqual(1, removed.Count);
			Assert.AreEqual(1, removed.Single().DeckId);
			Assert.IsNotNull(edited);
			Assert.AreEqual(0, edited.Count);
		}

		[TestMethod]
		public void Unloading()
		{
			UpdateCollection();
			LoadDecks();
			Assert.AreNotEqual(0, _collection.CardBacks.Count());
			Assert.AreNotEqual(0, _collection.Cards.Count());
			Assert.AreNotEqual(0, _collection.FavoriteHeroes.Count());
			Assert.AreNotEqual(0, _collection.Decks.Count());
			Assert.AreNotEqual(0, _collection.FavoriteCardBack);
			Assert.AreNotEqual(0, _collection.Dust);
			Assert.AreNotEqual(0, _collection.Gold);
			_collection.Unload();
			Assert.AreEqual(0, _collection.CardBacks.Count());
			Assert.AreEqual(0, _collection.Cards.Count());
			Assert.AreEqual(0, _collection.FavoriteHeroes.Count());
			Assert.AreEqual(0, _collection.Decks.Count());
			Assert.AreEqual(0, _collection.FavoriteCardBack);
			Assert.AreEqual(0, _collection.Dust);
			Assert.AreEqual(0, _collection.Gold);
		}
	}
}