using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone.Secrets
{
	public static class SecretList
	{
		private static readonly Dictionary<CardClass, IEnumerable<HearthDb.Card>> _secrets;

		static SecretList()
		{
			_secrets = Cards.Collectible.Values.Where(x => x.Type == CardType.SPELL && x.Entity.GetTag(GameTag.SECRET) > 0)
				.GroupBy(x => x.Class).ToDictionary(x => x.Key, x => x.OfType<HearthDb.Card>());
		}

		public static IEnumerable<HearthDb.Card> Get(int cardClass)
			=> Get((CardClass)cardClass);

		public static IEnumerable<HearthDb.Card> Get(CardClass cardClass)
			=> _secrets.TryGetValue(cardClass, out var secrets) ? secrets : new HearthDb.Card[0];
	}
}
