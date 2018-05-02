using System;

namespace HearthSim.Util.Caching
{
	public class CardTileImageCache : BitmapImageCache
	{
		private static readonly Func<string, string> KeyToUrl = cardId => $"https://art.hearthstonejson.com/v1/tiles/{cardId}.jpg";

		public CardTileImageCache(string persistentDirectory) : base(500, persistentDirectory, KeyToUrl)
		{
		}
	}
}
