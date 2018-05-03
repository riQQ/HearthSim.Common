using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using HearthDb;
using HearthDb.Enums;

namespace HearthSim.Util.Caching
{
	public class FullCardImageCache : BitmapImageCache
	{
		private const int DefaultResolution = 256;
		private const Locale DefaultLocale = Locale.enUS;

		private static readonly int[] ValidResolutions = {DefaultResolution, 512};

		private static Locale GetValidLocale(Locale locale)
		{
			if(locale == Locale.UNKNOWN || locale == Locale.enGB)
				return DefaultLocale;
			return locale;
		}

		private static int GetValidResolution(int resolution)
		{
			if(!ValidResolutions.Contains(resolution))
				return DefaultResolution;
			return resolution;
		}

		private static Func<string, string> GetKeyConverter(Locale locale, int resolution)
		{
			locale = GetValidLocale(locale);
			resolution = GetValidResolution(resolution);
			return cardId => $"https://art.hearthstonejson.com/v1/render/latest/{locale}/{resolution}x/{cardId}.png";
		}

		private static string GetDirectory(string baseDirectory, Locale locale)
		{
			locale = GetValidLocale(locale);
			return Path.Combine(baseDirectory, locale.ToString());
		}

		protected override BitmapImage GetPlaceholder(string cardId)
		{
			if(!Cards.All.TryGetValue(cardId, out var card))
				return new BitmapImage();
			return new BitmapImage(
				new Uri($"pack://application:,,,/HearthSim.Util;Component/Resources/{GetPlaceholderFile(card.Type)}"));
		}

		private string GetPlaceholderFile(CardType cardType)
		{
			switch(cardType)
			{
				case CardType.HERO:
					return "loading_hero.png";
				case CardType.MINION:
					return "loading_minion.png";
				case CardType.WEAPON:
					return "loading_weapon.png";
				default:
					return "loading_spell.png";
			}
		}

		public FullCardImageCache(string persistentDirectory, Locale locale = DefaultLocale, int resolution = DefaultResolution)
			: base(100, GetDirectory(persistentDirectory, locale), GetKeyConverter(locale, resolution))
		{
		}
	}
}
