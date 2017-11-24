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
		public event Action HearthstoneRestartRequired;
		public event Action<LogConfigErrorEventArgs> LogConfigError;

		// Menu
		public event Action<ModeChangedEventArgs> ModeChanged;
		public event Action GameCreated;
		public event Action GameEnded;
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;
		public event Action PackOpened;
		public event Action SpectatingStarted;
		public event Action SpectatingEnded;

		// Decks
		public event Action<DeckEditedEventArgs> DeckEdited;
		public event Action<DeckDeletedEventArgs> DeckDeleted;
		public event Action<QueuedForGameEventArgs> QueuedForGame; // deck or hero id
		public event Action<ConstructedDeckFoundEventArgs> ConstructedDecksFound;

		// Arena
		public event Action ArenaDraftComplete;
		public event Action ArenaDraftChoices;
		public event Action ArenaDraftPick;
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
		internal virtual void OnDeckEdited(DeckEditedEventArgs args) => DeckEdited?.Invoke(args);
		internal virtual void OnQueuedForGame(QueuedForGameEventArgs args) => QueuedForGame?.Invoke(args);
		internal virtual void OnConstructedDecksFound(ConstructedDeckFoundEventArgs args) => ConstructedDecksFound?.Invoke(args);
		internal virtual void OnDeckDeleted(DeckDeletedEventArgs args) => DeckDeleted?.Invoke(args);
		internal virtual void OnGoldProgressWins(GoldProgessWinsEventArgs args) => GoldProgressWins?.Invoke(args);
	}
}
