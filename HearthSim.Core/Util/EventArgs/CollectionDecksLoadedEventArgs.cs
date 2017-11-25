using System.Collections.Generic;
using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class CollectionDecksLoadedEventArgs : System.EventArgs
	{
		public CollectionDecksLoadedEventArgs(IEnumerable<Deck> decks)
		{
			Decks = decks;
		}

		public IEnumerable<Deck> Decks { get; }
	}
}
