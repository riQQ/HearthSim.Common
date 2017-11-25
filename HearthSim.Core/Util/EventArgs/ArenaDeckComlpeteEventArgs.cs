using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class ArenaDeckComlpeteEventArgs : System.EventArgs
	{
		public ArenaInfo Info { get; set; }

		public ArenaDeckComlpeteEventArgs(ArenaInfo info)
		{
			Info = info;
		}
	}
}
