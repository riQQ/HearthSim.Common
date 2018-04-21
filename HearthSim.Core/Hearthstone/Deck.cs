using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Util;
using DeckType = HearthSim.Core.Hearthstone.Enums.DeckType;

namespace HearthSim.Core.Hearthstone
{
	public class Deck
	{
		public DeckType Type { get; }
		private FormatType? _format;
		private string _deckstring;

		public Deck(DeckType type, HearthMirror.Objects.Deck deck)
		{
			Type = type;
			Name = deck.Name;
			DeckId = deck.Id;
			Cards = CardSorting.Sort(deck.Cards.Select(x => new Card(x)));
			Class = HearthDb.Cards.All.TryGetValue(deck.Hero, out var hero) ? hero.Class : CardClass.INVALID;
		}

		public Deck(DeckType type, HearthDb.Deckstrings.Deck deck)
		{
			Type = type;
			Name = deck.Name;
			DeckId = deck.DeckId;
			Cards = CardSorting.Sort(deck.GetCards().Select(card => new Card(card.Key, card.Value)));
			Class = deck.GetHero().Class;
		}

		public Deck(DeckType type, string name, CardClass cardClass, IEnumerable<string> cards)
		{
			Type = type;
			Name = name;
			Class = cardClass;
			Cards = CardSorting.Sort(cards.GroupBy(x => x).Select(x => new Card(x.Key, x.Count())));
		}

		public Deck(DeckType type, string name, CardClass cardClass, IEnumerable<int> cards)
		{
			Type = type;
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

		public string GetDeckstring()
		{
			return _deckstring ?? (_deckstring = HearthDb.Deckstrings.DeckSerializer.Serialize(new HearthDb.Deckstrings.Deck
			{
				Name = Name,
				Format = Format,
				CardDbfIds = Cards.Where(x => x.Data?.DbfId > 0).ToDictionary(x => x.Data.DbfId, x => x.Count),
				DeckId = DeckId,
				HeroDbfId = HearthDb.Cards.Collectible.Values.FirstOrDefault(x => x.Class == Class && x.Type == CardType.HERO)
								?.DbfId
							?? 0
			}, false));
		}

		public override string ToString()
		{
			return $"[Name={Name}, Class={Class}, DeckId={DeckId}, Format={Format}, Deckstring={GetDeckstring()}]";
		}
	}
}
