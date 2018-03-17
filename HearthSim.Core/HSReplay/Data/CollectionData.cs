using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HearthSim.Core.HSReplay.Data
{
	public class CollectionData
	{
		public CollectionData(IDictionary<int, int[]> cards, IDictionary<int, int> favoriteHeroes,
			IEnumerable<int> cardBacks, int favoriteCardBack, int dust)
		{
			Cards = new SortedDictionary<int, int[]>(cards);
			FavoriteHeroes = new SortedDictionary<int, int>(favoriteHeroes);
			CardBacks = cardBacks.OrderBy(x => x).ToList();
			FavoriteCardBack = favoriteCardBack;
			Dust = dust;
		}

		[JsonProperty("collection")]
		public SortedDictionary<int, int[]> Cards { get; }

		[JsonProperty("favorite_heroes")]
		public SortedDictionary<int, int> FavoriteHeroes { get; }

		[JsonProperty("cardbacks")]
		public List<int> CardBacks { get; }

		[JsonProperty("favorite_cardback")]
		public int FavoriteCardBack { get; }

		[JsonProperty("dust")]
		public int Dust { get; }

		public override int GetHashCode() => JsonConvert.SerializeObject(this).GetHashCode();
	}
}
