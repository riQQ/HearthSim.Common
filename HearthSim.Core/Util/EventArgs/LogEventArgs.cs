using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.Util.EventArgs
{
	public class LogEventArgs : System.EventArgs
	{
		public LogEventArgs(Line line)
		{
			Line = line;
		}

		public Line Line { get; }
	}
}
