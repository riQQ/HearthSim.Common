using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.Util.EventArgs
{
	public class GameStateLogEventArgs : System.EventArgs
	{
		public GameStateLogEventArgs(Line line)
		{
			Lines = line;
		}

		public Line Lines { get; }
	}
}
