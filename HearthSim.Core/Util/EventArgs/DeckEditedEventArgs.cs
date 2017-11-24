using HearthDb.Deckstrings;

namespace HearthSim.Core.Util.EventArgs
{
	public class DeckEditedEventArgs
	{
		public DeckEditedEventArgs(Deck deck)
		{
			Deck = deck;
		}

		public Deck Deck { get; }
	}
}
