#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Media.Imaging;
using HearthSim.Core.Hearthstone;
using HearthSim.UI.Extensions;

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

		public static readonly Dictionary<string, Bitmap> CardBitmaps = new Dictionary<string, Bitmap>();
		public static readonly Dictionary<string, BitmapImage> CardBitmapImages = new Dictionary<string, BitmapImage>();
		public static readonly List<string> LoadedSets = new List<string>();

		public static BitmapImage GetCardImage(Card card)
		{
			if(CardBitmapImages.TryGetValue(card.Id, out BitmapImage bmpImg))
				return bmpImg;
			if(!CardBitmaps.TryGetValue(card.Id, out Bitmap bmp))
				LoadResource(card, out bmp);
			if(bmp != null)
				bmpImg = bmp.ToImageSource();
			CardBitmapImages.Add(card.Id, bmpImg);
			return bmpImg;
		}

		public static Bitmap GetCardBitmap(Card card)
		{
			if(!CardBitmaps.TryGetValue(card.Id, out Bitmap bmp))
				LoadResource(card, out bmp);
			return bmp;
		}

		private static void LoadResource(Card card, out Bitmap bitmap)
		{
			bitmap = null;
			var set = card.Data.Set + (card.Data.Collectible ? "" : "_NC");
			if(LoadedSets.Contains(set))
				return;
			LoadedSets.Add(set);
			var file = new FileInfo($"Images/Tiles/{set}.res");
			if(!file.Exists)
				return;
			using(var reader = new ResourceReader(file.FullName))
			{
				foreach(var entry in reader.OfType<DictionaryEntry>())
				{
					var key = entry.Key.ToString();
					if(key == card.Id)
						bitmap = (Bitmap)entry.Value;
					CardBitmaps.Add(key, (Bitmap)entry.Value);
				}
			}
		}
	}
}
