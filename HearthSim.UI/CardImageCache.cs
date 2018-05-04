using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HearthSim.Util.Logging;
using HearthSim.UI.Themes;
using HearthSim.UI.Util;
using HearthSim.Util.Caching;

namespace HearthSim.UI
{
	public static class CardImageCache
	{
		public static event Action<string> CardImageUpdated;
		static CardImageCache()
		{
			ImageCache.CardTileUpdated += cardId =>
			{
				Refresh.Add(cardId);
				CardImageUpdated?.Invoke(cardId);
			};
		}

		private static readonly MemCache<CardImageObject> Cache = new MemCache<CardImageObject>(500);

		private static readonly List<string> Refresh = new List<string>();

		public static DrawingBrush Get(CardViewModel card)
		{
			try
			{
				if(card?.Id == null || card.Name == null || Cache == null)
					return new DrawingBrush();
				var cardImageObj = new CardImageObject(card);
				var key = $"{card.Id}_{cardImageObj.GetHashCode()}";
				if(!Refresh.Contains(card.Id) && Cache.TryGet(key, out var cached))
					return cached.Image;
				Refresh.Remove(card.Id);
				DrawingBrush image = null;
				Application.Current.Dispatcher.Invoke(() => image = ThemeManager.GetBarImageBuilder(card).Build());
				if(image.CanFreeze)
					image.Freeze();
				cardImageObj = new CardImageObject(image, card);
				Cache.Cache(key, cardImageObj);
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
