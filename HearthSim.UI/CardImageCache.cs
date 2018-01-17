using System;
using System.Collections.Generic;
using System.Windows.Media;
using HearthSim.Core.Util.Logging;
using HearthSim.UI.Themes;

namespace HearthSim.UI
{
	public static class CardImageCache
	{
		private static readonly Dictionary<string, Dictionary<int, CardImageObject>> Cache =
			new Dictionary<string, Dictionary<int, CardImageObject>>();

		public static DrawingBrush Get(CardViewModel card)
		{
			try
			{
				if(card?.Id == null || card.Name == null || Cache == null)
					return new DrawingBrush();
				var cardImageObj = new CardImageObject(card);
				if(Cache.TryGetValue(card.Id, out Dictionary<int, CardImageObject> cache))
				{
					if(cache.TryGetValue(cardImageObj.GetHashCode(), out CardImageObject cached))
						return cached.Image;
				}
				var image = ThemeManager.GetBarImageBuilder(card).Build();
				if(image.CanFreeze)
					image.Freeze();
				cardImageObj = new CardImageObject(image, card);
				if(cache == null)
				{
					cache = new Dictionary<int, CardImageObject>();
					Cache.Add(card.Id, cache);
				}
				cache.Add(cardImageObj.GetHashCode(), cardImageObj);
				return cardImageObj.Image;
			}
			catch(Exception ex)
			{
				Log.Error($"Image builder failed: {ex.Message}");
				return new DrawingBrush();
			}
		}
	}
}
