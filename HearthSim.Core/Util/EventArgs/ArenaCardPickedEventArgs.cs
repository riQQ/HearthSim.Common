using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class ArenaCardPickedEventArgs : System.EventArgs
	{
		public Card Picked { get; }
		public Card[] Choices { get; }

		public ArenaCardPickedEventArgs(Card picked, Card[] choices)
		{
			Picked = picked;
			Choices = choices;
		}
	}
}
