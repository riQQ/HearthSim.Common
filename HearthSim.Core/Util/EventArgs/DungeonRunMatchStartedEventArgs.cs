using Deck = HearthSim.Core.Hearthstone.Deck;

namespace HearthSim.Core.Util.EventArgs
{
	public class DungeonRunMatchStartedEventArgs : System.EventArgs
	{
		public bool IsNew { get; }
		public Deck Deck { get; }

		public DungeonRunMatchStartedEventArgs(bool isNew, Deck deck)
		{
			IsNew = isNew;
			Deck = deck;
		}
	}
}
