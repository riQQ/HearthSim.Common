using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class PackOpenedEventArgs : System.EventArgs
	{
		public PackOpenedEventArgs(List<Card> cards, int packId)
		{
			Cards = cards;
			PackId = packId;
		}

		public List<Card> Cards { get; }
		public int PackId { get; set; }
	}
}
