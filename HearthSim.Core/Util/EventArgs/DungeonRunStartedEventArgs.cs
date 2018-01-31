using Deck = HearthSim.Core.Hearthstone.Deck;

namespace HearthSim.Core.Util.EventArgs
{
	public class DungeonRunStartedEventArgs : System.EventArgs
	{
		public bool IsNew { get; }
		public Deck Deck { get; }

		public DungeonRunStartedEventArgs(bool isNew, Deck deck)
		{
			IsNew = isNew;
			Deck = deck;
		}
	}
}
