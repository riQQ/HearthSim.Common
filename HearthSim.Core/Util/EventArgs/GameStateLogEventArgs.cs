using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameStateLogEventArgs : System.EventArgs
	{
		public GameStateLogEventArgs(Line line)
		{
			Line = line;
		}

		public Line Line { get; }
	}
}
