using HearthDb.Deckstrings;

namespace HearthSim.Core.Util.EventArgs
{
	public class QueuedForGameEventArgs : System.EventArgs
	{
		public Deck Deck { get; }
		public int HeroId { get; }

		public QueuedForGameEventArgs(Deck deck)
		{
			Deck = deck;
		}

		public QueuedForGameEventArgs(int heroId)
		{
			HeroId = heroId;
		}
	}
}
