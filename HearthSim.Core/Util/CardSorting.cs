using System.Collections.Generic;
using System.Linq;

namespace HearthSim.Core.Util
{
	public static class CardSorting
	{
		public static IOrderedEnumerable<T> Sort<T>(IEnumerable<T> cards) where T : ISortableCard
		{
			return cards.OrderBy(c => c.Cost).ThenBy(c => c.Name);
		}
	}
}
