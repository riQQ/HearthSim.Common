﻿using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogReading
{
	public static class LogWatcherConfigs
	{
		internal static LogWatcherData Power => 
			new LogWatcherData("Power", new[] {"tag=GOLD_REWARD_STATE", "End Spectator"},
				s => s.StartsWith("PowerTaskList.DebugPrintPower") || s.StartsWith("GameState."),
				s => s.Contains("Spectating") || s.Contains("Spectator"));

		internal static LogWatcherData LoadingScreen =>
			new LogWatcherData("LoadingScreen", new[] {"Gameplay.Start"},
				s => s.StartsWith("LoadingScreen.OnSceneLoaded") || s.StartsWith("Gameplay."));

		public static LogWatcherData Rachelle => new LogWatcherData("Rachelle");

		public static LogWatcherData Arena => new LogWatcherData("Arena");

		internal static LogWatcherData Decks => new LogWatcherData("Decks", new []{"Game With Deck:"});
	}
}
