using HearthSim.Core.Hearthstone.Enums;

namespace HearthSim.Core.LogParsing.Parsers.LoadingScreen
{
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
}
