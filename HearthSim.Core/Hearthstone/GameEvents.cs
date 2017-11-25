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
		public event Action GameCreated;
		public event Action GameEnded;
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;
		public event Action<PackOpenedEventArgs> PackOpened;
		public event Action SpectatingStarted; // TODO - find log line in powerlog?
		public event Action SpectatingEnded;

		// Decks
		public event Action<QueuedForGameEventArgs> QueuedForGame;

		// Arena
		public event Action<ArenaDeckComlpeteEventArgs> ArenaDraftComplete;
		public event Action<ArenaChoicesChangedEventArgs> ArenaDraftChoices;
		public event Action<ArenaCardPickedEventArgs> ArenaDraftPick;
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
		internal virtual void OnArenaDraftComplete(ArenaDeckComlpeteEventArgs args) => ArenaDraftComplete?.Invoke(args);
		internal virtual void OnArenaDraftChoices(ArenaChoicesChangedEventArgs args) => ArenaDraftChoices?.Invoke(args);
		internal virtual void OnArenaDraftPick(ArenaCardPickedEventArgs args) => ArenaDraftPick?.Invoke(args);
		internal virtual void OnSpectatorStart() => SpectatingStarted?.Invoke();
		internal virtual void OnSpectatorEnd() => SpectatingEnded?.Invoke();
		internal virtual void OnQueuedForGame(QueuedForGameEventArgs args) => QueuedForGame?.Invoke(args);
		internal virtual void OnGoldProgressWins(GoldProgessWinsEventArgs args) => GoldProgressWins?.Invoke(args);
		internal virtual void OnHearthstoneStarted() => HearthstoneStarted?.Invoke();
		internal virtual void OnHearthstoneLoaded() => HearthstoneLoaded?.Invoke();
		internal virtual void OnHearthstoneExited() => HearthstoneExited?.Invoke();
		internal virtual void OnPackOpened(PackOpenedEventArgs args) => PackOpened?.Invoke(args);
	}
}
