using System;
using System.IO;
using System.Linq;
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

		public FullCardImageCache(string persistentDirectory, Locale locale = DefaultLocale, int resolution = DefaultResolution)
			: base(100, GetDirectory(persistentDirectory, locale), GetKeyConverter(locale, resolution))
		{
		}
	}
}
