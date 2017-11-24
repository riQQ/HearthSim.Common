namespace HearthSim.Core.Util.EventArgs
{
	public class DeckDeletedEventArgs : System.EventArgs
	{
		public DeckDeletedEventArgs(long deckId)
		{
			DeckId = deckId;
		}

		public long DeckId { get; }
	}
}
