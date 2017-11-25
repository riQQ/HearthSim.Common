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
		public event Action HearthstoneStarted; // TODO - move processwatcher up from log reader
		public event Action HearthstoneExited; // TODO - move processwatcherup from log reader
		public event Action HearthstoneRestartRequired;
		public event Action<LogConfigErrorEventArgs> LogConfigError;

		// Menu
		public event Action<ModeChangedEventArgs> ModeChanged;
		public event Action GameCreated;
		public event Action GameEnded;
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;
		public event Action PackOpened; // TODO - start pack watcher when in PACK_OPENING scene
		public event Action SpectatingStarted; // TODO - find log line in powerlog?
		public event Action SpectatingEnded;

		// Decks
		public event Action<QueuedForGameEventArgs> QueuedForGame;

		// Arena
		public event Action ArenaDraftComplete; // TODO - start arena watcher when in FORGE scene
		public event Action ArenaDraftChoices; // TODO - start arena watcher when in FORGE scene
		public event Action ArenaDraftPick; // TODO - start arena watcher when in FORGE scene
		public event Action<ArenaRunCompleteEventArgs> ArenaRunComplete;

		// Game
		public event Action<GameStateChangedEventArgs> GameStateChanged;

		internal virtual void OnModeChanged(ModeChangedEventArgs args) => ModeChanged?.Invoke(args);
		internal virtual void OnGameStateChanged(GameStateChangedEventArgs args) => GameStateChanged?.Invoke(args);
		internal virtual void OnCreateGame() => GameCreated?.Invoke();
		internal virtual void OnHearthstoneRestartRequired() => HearthstoneRestartRequired?.Invoke();
		internal virtual void OnLogConfigError(LogConfigErrorEventArgs args) => LogConfigError?.Invoke(args);
		internal virtual void OnGameEnded() => GameEnded?.Invoke();
		internal virtual void OnArenaRunComplete(ArenaRunCompleteEventArgs args) => ArenaRunComplete?.Invoke(args);
		internal virtual void OnSpectatorStart() => SpectatingStarted?.Invoke();
		internal virtual void OnSpectatorEnd() => SpectatingEnded?.Invoke();
		internal virtual void OnQueuedForGame(QueuedForGameEventArgs args) => QueuedForGame?.Invoke(args);
		internal virtual void OnGoldProgressWins(GoldProgessWinsEventArgs args) => GoldProgressWins?.Invoke(args);
	}
}
