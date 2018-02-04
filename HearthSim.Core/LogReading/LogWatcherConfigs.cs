using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogReading
{
	public static class LogWatcherConfigs
	{
		internal static LogWatcherData Power => 
			new LogWatcherData("Power", new[] {"tag=STATE value=COMPLETE", "GOLD_REWARD_STATE", "End Spectator"},
				s => s.StartsWith("PowerTaskList.") || s.StartsWith("GameState."),
				s => s.Contains("Spectating") || s.Contains("Spectator"));

		internal static LogWatcherData LoadingScreen =>
			new LogWatcherData("LoadingScreen", new[] {"currMode=FATAL_ERROR"},
				s => s.StartsWith("LoadingScreen.OnSceneLoaded") || s.StartsWith("Gameplay."));

		public static LogWatcherData Rachelle => new LogWatcherData("Rachelle");

		public static LogWatcherData Arena => new LogWatcherData("Arena");

		internal static LogWatcherData Decks => new LogWatcherData("Decks", new []{"Game With Deck:", "Game With Hero:"});
	}
}
