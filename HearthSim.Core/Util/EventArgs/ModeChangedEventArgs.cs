using HearthSim.Core.Hearthstone.Enums;

namespace HearthSim.Core.Util.EventArgs
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
