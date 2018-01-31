using Deck = HearthSim.Core.Hearthstone.Deck;

namespace HearthSim.Core.Util.EventArgs
{
	public class DungeonDeckUpdatedEventArgs : System.EventArgs
	{
		public Deck Deck { get; }

		public DungeonDeckUpdatedEventArgs(Deck deck)
		{
			Deck = deck;
		}
	}
}
