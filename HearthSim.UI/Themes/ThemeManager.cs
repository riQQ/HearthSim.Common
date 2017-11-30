using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HearthSim.Core.Util.Logging;

namespace HearthSim.UI.Themes
{
	public class ThemeManager
	{
		private const string ThemeDir = @"Images\Themes\Bars";
		private const string ThemeRegex = @"[a-zA-Z]+";

		public static List<Theme> Themes = new List<Theme>();
		internal static ThemeConfig Config;

		public static Theme CurrentTheme { get; private set; }
		public static event Action ThemeChanged;

		public static void Load(ThemeConfig config)
		{
			Config = config;
			if(config.CustomThemeDirectory != null)
				LoadThemes(Path.Combine(config.CustomThemeDirectory, @"Themes\Bars"));
			LoadThemes(ThemeDir);
			CurrentTheme = FindTheme(config.Theme) ?? Themes.FirstOrDefault();
		}

		private static void LoadThemes(string dir)
		{
			var dirInfo = new DirectoryInfo(dir);
			if(!dirInfo.Exists)
				return;
			foreach(var di in dirInfo.GetDirectories())
			{
				if(Regex.IsMatch(di.Name, ThemeRegex))
				{
					Log.Info($"Found theme: {di.Name}");
					Themes.Add(new Theme(di.Name, di.FullName, GetBuilderType(di.Name)));
				}
				else
				{
					Log.Warn($"Invalid theme directory name {di.Name}");
				}
			}
		}

		public static Theme FindTheme(string name)
			=> string.IsNullOrWhiteSpace(name) ? null : Themes.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());

		public static void SetTheme(string theme)
		{
			var t = Themes.FirstOrDefault(x => x.Name.ToLowerInvariant() == theme.ToLowerInvariant());
			if(t == null)
				return;
			CurrentTheme = t;
			ThemeChanged?.Invoke();
		}

		public static CardBarImageBuilder GetBarImageBuilder(CardViewModel card)
		{
			var buildType = CurrentTheme.BuildType ?? typeof(DefaultBarImageBuilder);
			return (CardBarImageBuilder)Activator.CreateInstance(buildType, Config, card, CurrentTheme.Directory);
		}

		private static Type GetBuilderType(string name)
		{
			string className = null;
			if(!string.IsNullOrWhiteSpace(name))
			{
				className = name[0].ToString().ToUpperInvariant();
				if(name.Length > 1)
					className += name.ToLowerInvariant().Substring(1);
				className += "BarImageBuilder";
			}

			Type buildType = null;
			try
			{
				buildType = Type.GetType("HearthSim.UI.Themes." + className);
			}
			catch(Exception)
			{
				Log.Warn($"Theme builder {className} not found, using default.");
			}
			return buildType;
		}
	}

	public class ThemeConfig
	{
		public string Theme { get; set; }
		public string CustomThemeDirectory { get; set; }
		public bool RarityCardFrames { get; set; }
		public bool RarityCardGems { get; set; }
		public bool IndicateDiscarded { get; set; }
	}
}
