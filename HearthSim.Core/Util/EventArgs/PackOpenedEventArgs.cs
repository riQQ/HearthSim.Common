using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.EventArgs
{
	public class PackOpenedEventArgs : System.EventArgs
	{
		public PackOpenedEventArgs(Pack pack)
		{
			Pack = pack;
		}

		public Pack Pack { get; }
	}
}
