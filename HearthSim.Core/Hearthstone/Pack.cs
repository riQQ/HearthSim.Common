using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone
{
	public class Pack
	{
		public Pack(Booster boosterType, IEnumerable<CollectionCard> cards)
		{
			BoosterType = boosterType;
			Cards = cards.ToList().AsReadOnly();
		}

		public Booster BoosterType { get; }
		public IReadOnlyCollection<CollectionCard> Cards { get; }
	}
}
