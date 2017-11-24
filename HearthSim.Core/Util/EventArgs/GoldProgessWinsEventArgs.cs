namespace HearthSim.Core.Util.EventArgs
{
	public class GoldProgessWinsEventArgs : System.EventArgs
	{
		public GoldProgessWinsEventArgs(int wins)
		{
			Wins = wins;
		}

		public int Wins { get; }
	}
}
