#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.UI.Extensions;
using HearthSim.Util.Caching;
using HearthSim.Util.Extensions;

#endregion

namespace HearthSim.UI.Util
{
	public class ImageCache
	{
		private static readonly Dictionary<string, BitmapImage> ImageCacheDict = new Dictionary<string, BitmapImage>();

		public static BitmapImage GetImage(string resourcePath, string basePath = "Resources")
		{
			if(ImageCacheDict.TryGetValue(resourcePath, out var image))
				return image;
			var uri = new Uri($"pack://application:,,,/{basePath}/{resourcePath}", UriKind.Absolute);
			image = new BitmapImage(uri);
			ImageCacheDict.Add(resourcePath, image);
			return image;
		}

		public static void Initialize(string directory)
		{
			CardTileCache = new CardTileImageCache(Path.Combine(directory, "Tiles"));
			FullCardCache  = new FullCardImageCache(directory);
			
		}

		private static CardTileImageCache CardTileCache;
		private static FullCardImageCache FullCardCache;

		public static event Action<string> CardTileUpdated;
		public static event Action<string> FullCardUpdated;

		public static async Task<BitmapImage> GetTile(string cardId)
		{
			if(CardTileCache == null)
				return null;
			return string.IsNullOrEmpty(cardId) ? null : await CardTileCache.Get(cardId);
		}

		public static BitmapImage TryGetTile(string cardId)
		{
			if(CardTileCache == null)
				return null;
			if(CardTileCache.TryGet(cardId, out var image))
				return image;
			Task.Run(async () =>
			{
				image = await CardTileCache.Get(cardId);
				CardTileUpdated?.Invoke(cardId);
			}).Forget();
			return image;
		}

		public static bool HasCardTile(string cardId)
		{
			return CardTileCache != null && CardTileCache.TryGet(cardId, out var img);
		}

		public static async Task<BitmapImage> GetFullImage(string cardId)
		{
			if(FullCardCache == null)
				return null;
			return string.IsNullOrEmpty(cardId) ? null : await FullCardCache.Get(cardId);
		}

		public static BitmapImage TryGetFullImage(string cardId)
		{
			if(FullCardCache == null)
				return null;
			if(FullCardCache.TryGet(cardId, out var image))
				return image;
			Task.Run(async () =>
			{
				image = await FullCardCache.Get(cardId);
				FullCardUpdated?.Invoke(cardId);
			}).Forget();
			return image;
		}
	}
}
