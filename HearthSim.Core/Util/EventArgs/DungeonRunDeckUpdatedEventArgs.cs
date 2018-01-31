using Deck = HearthSim.Core.Hearthstone.Deck;

namespace HearthSim.Core.Util.EventArgs
{
	public class DungeonRunDeckUpdatedEventArgs : System.EventArgs
	{
		public Deck Deck { get; }

		public DungeonRunDeckUpdatedEventArgs(Deck deck)
		{
			Deck = deck;
		}
	}
}
