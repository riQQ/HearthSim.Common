using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class CollectionDeckChangedEventArgs : System.EventArgs
	{
		public CollectionDeckChangedEventArgs(Deck deck)
		{
			Deck = deck;
		}

		public Deck Deck { get; }
	}
}
