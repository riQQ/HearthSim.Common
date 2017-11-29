using System;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class GameEvents
	{
		protected GameEvents()
		{
		}

		// Process
		public event Action HearthstoneStarted;
		public event Action HearthstoneExited;
		public event Action HearthstoneLoaded;
		public event Action HearthstoneRestartRequired;
		public event Action<LogConfigErrorEventArgs> LogConfigError;

		// Menu
		public event Action<ModeChangedEventArgs> ModeChanged;
		public event Action<GameCreatedEventArgs> GameCreated;
		public event Action<GameEndEventArgs> GameEnded;
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;
		public event Action<PackOpenedEventArgs> PackOpened;

		// Decks
		public event Action<QueuedForGameEventArgs> QueuedForGame;

		// Game
		public event Action<GameStateChangedEventArgs> GameStateChanged;

		internal virtual void OnModeChanged(ModeChangedEventArgs args) => ModeChanged?.Invoke(args);
		internal virtual void OnGameStateChanged(GameStateChangedEventArgs args) => GameStateChanged?.Invoke(args);
		internal virtual void OnCreateGame(GameCreatedEventArgs args) => GameCreated?.Invoke(args);
		internal virtual void OnHearthstoneRestartRequired() => HearthstoneRestartRequired?.Invoke();
		internal virtual void OnLogConfigError(LogConfigErrorEventArgs args) => LogConfigError?.Invoke(args);
		internal virtual void OnGameEnded(GameEndEventArgs args) => GameEnded?.Invoke(args);
		internal virtual void OnQueuedForGame(QueuedForGameEventArgs args) => QueuedForGame?.Invoke(args);
		internal virtual void OnGoldProgressWins(GoldProgessWinsEventArgs args) => GoldProgressWins?.Invoke(args);
		internal virtual void OnHearthstoneStarted() => HearthstoneStarted?.Invoke();
		internal virtual void OnHearthstoneLoaded() => HearthstoneLoaded?.Invoke();
		internal virtual void OnHearthstoneExited() => HearthstoneExited?.Invoke();
		internal virtual void OnPackOpened(PackOpenedEventArgs args) => PackOpened?.Invoke(args);
	}
}
