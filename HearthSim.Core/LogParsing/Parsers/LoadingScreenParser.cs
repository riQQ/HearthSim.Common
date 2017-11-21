using System;
using System.Text.RegularExpressions;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class LoadingScreenParser : ILogParser
	{
		private readonly Regex _modeRegex = new Regex(@"prevMode=(?<prev>(\w+)).*currMode=(?<curr>(\w+))");
		public string LogName { get; } = "LoadingScreen";

		public void Parse(Line line)
		{
			var match = _modeRegex.Match(line.Text);
			if(match.Success)
			{
				var currentMode = GetMode(match.Groups["curr"].Value);
				var previousMode = GetMode(match.Groups["prev"].Value);
				ModeChanged?.Invoke(new ModeChangedEventArgs(currentMode, previousMode));
			}
			else if(line.Text.Contains("Gameplay.Start"))
				GameplayStart?.Invoke();
		}

		public event Action GameplayStart;
		public event Action<ModeChangedEventArgs> ModeChanged;

		private Mode GetMode(string modeString) => Enum.TryParse(modeString, out Mode mode) ? mode : Mode.INVALID;

		public class ModeChangedEventArgs
		{
			public ModeChangedEventArgs(Mode currentMode, Mode previousMode)
			{
				CurrentMode = currentMode;
				PreviousMode = previousMode;
			}

			public Mode PreviousMode { get; }

			public Mode CurrentMode { get; }
		}

		public enum Mode
		{
			INVALID,
			STARTUP,
			LOGIN,
			HUB,
			GAMEPLAY,
			COLLECTIONMANAGER,
			PACKOPENING,
			TOURNAMENT,
			FRIENDLY,
			FATAL_ERROR,
			DRAFT,
			CREDITS,
			RESET,
			ADVENTURE,
			TAVERN_BRAWL,
			FIRESIDE_GATHERING
		}
	}
}
