using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class ArenaRunCompleteEventArgs : System.EventArgs
	{
		public ArenaInfo Info { get; }

		public ArenaRunCompleteEventArgs(ArenaInfo info)
		{
			Info = info;
		}
	}
}
