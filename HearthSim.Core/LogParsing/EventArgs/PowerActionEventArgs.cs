namespace HearthSim.Core.LogParsing.EventArgs
{
	public class PowerActionEventArgs : System.EventArgs
	{
		public PowerAction Action { get; }

		public PowerActionEventArgs(PowerAction action)
		{
			Action = action;
		}
	}
}
