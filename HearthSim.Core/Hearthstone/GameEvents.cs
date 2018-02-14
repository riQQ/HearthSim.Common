using System;
using System.Collections.Generic;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class GameEvents
	{
		private readonly Dictionary<GameTagChange, Action<CommonGameEventArgs>> _actions;

		protected GameEvents()
		{
			_actions = new Dictionary<GameTagChange, Action<CommonGameEventArgs>>
			{
			};
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
		public event Action<DungeonRunMatchStartedEventArgs> DungeonRunMatchStarted;
		public event Action<DungeonRunDeckUpdatedEventArgs> DungeonRunDeckUpdated;

		// Game
		public event Action<GameStateChangedEventArgs> GameStateChanged;
		public event Action SetupComplete;
		public event Action<ActivePlayerDeckChangedEventArgs> ActivePlayerDeckChanged;

		internal virtual void OnSetupComplete() => SetupComplete?.Invoke();
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
		internal virtual void OnDungeonRunMatchStarted(DungeonRunMatchStartedEventArgs args) =>
			DungeonRunMatchStarted?.Invoke(args);
		internal virtual void OnDungeonRunDeckUpdated(DungeonRunDeckUpdatedEventArgs args) =>
			DungeonRunDeckUpdated?.Invoke(args);
		internal virtual void OnActivePlayerDeckChanged(ActivePlayerDeckChangedEventArgs args) =>
			ActivePlayerDeckChanged?.Invoke(args);

		internal void OnTagChange(TagChange t, IGameState state)
		{
			if(t.EntityId.HasValue && _actions.TryGetValue((t.Tag, t.PreviousValue ?? 0, t.Value), out var action))
				action?.Invoke(new CommonGameEventArgs(t.EntityId.Value, state));
		}
	}
}
