using System;
using System.Windows.Media.Imaging;

namespace HearthSim.Util.Caching
{
	public class CardTileImageCache : BitmapImageCache
	{
		private static readonly Func<string, string> KeyToUrl = cardId => $"https://art.hearthstonejson.com/v1/tiles/{cardId}.jpg";

		protected override BitmapImage GetPlaceholder(string key)
		{
			return new BitmapImage(new Uri("pack://application:,,,/HearthSim.Util;Component/Resources/loading_tile.jpg"));
		}

		public CardTileImageCache(string persistentDirectory) : base(500, persistentDirectory, KeyToUrl)
		{
		}
	}
}
