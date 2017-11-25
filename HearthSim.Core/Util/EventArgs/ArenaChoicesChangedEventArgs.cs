using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class ArenaChoicesChangedEventArgs : System.EventArgs
	{
		public Card[] Choices { get; }
		public Deck Deck { get; }

		public ArenaChoicesChangedEventArgs(Card[] choices, Deck deck)
		{
			Choices = choices;
			Deck = deck;
		}
	}
}
