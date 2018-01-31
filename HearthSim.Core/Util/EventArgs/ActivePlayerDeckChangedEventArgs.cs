using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class ActivePlayerDeckChangedEventArgs : System.EventArgs
	{
		public bool IsLocalPlayer { get; }
		public Deck Deck { get; }

		public ActivePlayerDeckChangedEventArgs(bool isLocalPlayer, Deck deck)
		{
			IsLocalPlayer = isLocalPlayer;
			Deck = deck;
		}
	}
}
