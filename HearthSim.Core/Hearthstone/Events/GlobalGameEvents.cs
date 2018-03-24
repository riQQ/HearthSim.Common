using System;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone.Events
{
	public class GlobalGameEvents
	{
		protected GlobalGameEvents()
		{
		}

		// Process
		public event Action HearthstoneStarted;
		public event Action HearthstoneExited;
		public event Action HearthstoneLoaded;
		public event Action HearthstoneRestartRequired;
		public event Action HearthstoneInstallationNotFound;
		public event Action<LogConfigErrorEventArgs> LogConfigError;

		// Menu
		public event Action<ModeChangedEventArgs> ModeChanged;
		public event Action<GameCreatedEventArgs> GameCreated;
		public event Action<GameEndEventArgs> GameEnded;
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;
		public event Action<PackOpenedEventArgs> PackOpened;
		public event Action FriendlyChallenge;

		// Decks
		public event Action<QueuedForGameEventArgs> QueuedForGame;
		public event Action<DungeonRunMatchStartedEventArgs> DungeonRunMatchStarted;
		public event Action<DungeonRunDeckUpdatedEventArgs> DungeonRunDeckUpdated;

		// Game
		public event Action<ActivePlayerDeckChangedEventArgs> ActivePlayerDeckChanged;

		internal virtual void OnModeChanged(ModeChangedEventArgs args) => ModeChanged?.Invoke(args);
		internal virtual void OnCreateGame(GameCreatedEventArgs args) => GameCreated?.Invoke(args);
		internal virtual void OnHearthstoneRestartRequired() => HearthstoneRestartRequired?.Invoke();
		internal virtual void OnLogConfigError(LogConfigErrorEventArgs args) => LogConfigError?.Invoke(args);
		internal virtual void OnGameEnded(GameEndEventArgs args) => GameEnded?.Invoke(args);
		internal virtual void OnQueuedForGame(QueuedForGameEventArgs args) => QueuedForGame?.Invoke(args);
		internal virtual void OnGoldProgressWins(GoldProgessWinsEventArgs args) => GoldProgressWins?.Invoke(args);
		internal virtual void OnHearthstoneStarted() => HearthstoneStarted?.Invoke();
		internal virtual void OnHearthstoneLoaded() => HearthstoneLoaded?.Invoke();
		internal virtual void OnHearthstoneExited() => HearthstoneExited?.Invoke();
		internal virtual void OnHearthstoneInstallationNotFound() => HearthstoneInstallationNotFound?.Invoke();
		internal virtual void OnPackOpened(PackOpenedEventArgs args) => PackOpened?.Invoke(args);
		internal virtual void OnDungeonRunMatchStarted(DungeonRunMatchStartedEventArgs args) =>
			DungeonRunMatchStarted?.Invoke(args);
		internal virtual void OnDungeonRunDeckUpdated(DungeonRunDeckUpdatedEventArgs args) =>
			DungeonRunDeckUpdated?.Invoke(args);
		internal virtual void OnActivePlayerDeckChanged(ActivePlayerDeckChangedEventArgs args) =>
			ActivePlayerDeckChanged?.Invoke(args);
		internal virtual void OnFriendlyChallenge() => FriendlyChallenge?.Invoke();
	}
}
