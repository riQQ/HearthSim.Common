using System.Collections.Generic;
using HearthDb.Deckstrings;

namespace HearthSim.Core.Util.EventArgs
{
	public class ConstructedDeckFoundEventArgs : System.EventArgs
	{
		public List<Deck> Decks { get; }

		public ConstructedDeckFoundEventArgs(List<Deck> decks)
		{
			Decks = decks;
		}
	}
}
