using System.Collections.Generic;
using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class CollectionCardChangedEventArgs : System.EventArgs
	{
		public CollectionCardChangedEventArgs(IEnumerable<CollectionCard> cards)
		{
			Cards = cards;
		}

		public IEnumerable<CollectionCard> Cards { get; }
	}
}
