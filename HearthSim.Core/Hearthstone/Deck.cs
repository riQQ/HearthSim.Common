using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Util;

namespace HearthSim.Core.Hearthstone
{
	public class Deck
	{
		private FormatType? _format;

		public Deck(HearthMirror.Objects.Deck deck)
		{
			Name = deck.Name;
			DeckId = deck.Id;
			Cards = CardSorting.Sort(deck.Cards.Select(x => new Card(x)));
			Class = HearthDb.Cards.All.TryGetValue(deck.Hero, out var hero) ? hero.Class : CardClass.INVALID;
		}

		public Deck(HearthDb.Deckstrings.Deck deck)
		{
			Name = deck.Name;
			DeckId = deck.DeckId;
			Cards = CardSorting.Sort(deck.GetCards().Select(card => new Card(card.Key, card.Value)));
			Class = deck.GetHero().Class;
		}

		public Deck(string name, CardClass cardClass, IEnumerable<string> cards)
		{
			Name = name;
			Class = cardClass;
			Cards = CardSorting.Sort(cards.GroupBy(x => x).Select(x => new Card(x.Key, x.Count())));
		}

		public Deck(string name, CardClass cardClass, IEnumerable<int> cards)
		{
			Name = name;
			Class = cardClass;
			Cards = CardSorting.Sort(cards.GroupBy(x => x).Select(x => new Card(x.Key, x.Count())));
		}

		public CardClass Class { get; }
		public IOrderedEnumerable<Card> Cards { get; }
		public string Name { get; }
		public long DeckId { get; }

		public FormatType Format
			=> (_format ?? (_format = Cards.Any(c => c.IsWild) ? FormatType.FT_WILD : FormatType.FT_STANDARD)).Value;
	}
}
