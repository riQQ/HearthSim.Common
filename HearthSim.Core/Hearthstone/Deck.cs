using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone
{
	public class Deck
	{
		private FormatType? _format;

		public Deck(HearthMirror.Objects.Deck deck)
		{
			Name = deck.Name;
			DeckId = deck.Id;
			Cards = Sort(deck.Cards.Select(x => new Card(x)));
			Class = HearthDb.Cards.All.TryGetValue(deck.Hero, out var hero) ? hero.Class : CardClass.INVALID;
		}

		public Deck(HearthDb.Deckstrings.Deck deck)
		{
			Name = deck.Name;
			DeckId = deck.DeckId;
			Cards = Sort(deck.GetCards().Select(card => new Card(card.Key, card.Value)));
			Class = deck.GetHero().Class;
		}

		public CardClass Class { get; }
		public IOrderedEnumerable<Card> Cards { get; }
		public string Name { get; }
		public long DeckId { get; }

		public FormatType Format
			=> (_format ?? (_format = Cards.Any(c => c.IsWild) ? FormatType.FT_WILD : FormatType.FT_STANDARD)).Value;

		private IOrderedEnumerable<Card> Sort(IEnumerable<Card> cards)
		{
			return cards.OrderBy(c => c.Data?.Cost).ThenBy(c => c.Data?.Name);
		}
	}
}
