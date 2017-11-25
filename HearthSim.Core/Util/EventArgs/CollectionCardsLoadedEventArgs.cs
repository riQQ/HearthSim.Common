using System.Collections.Generic;
using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class CollectionCardsLoadedEventArgs : System.EventArgs
	{
		public CollectionCardsLoadedEventArgs(IReadOnlyCollection<CollectionCard> cards)
		{
			Cards = cards;
		}

		public IReadOnlyCollection<CollectionCard> Cards { get; }
	}
}
