using System;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone.Events
{
	public class GlobalGameEvents
	{
		protected GlobalGameEvents()
		{
		}

		/// <summary>
		/// Hearthstone Process started
		/// </summary>
		public event Action HearthstoneStarted;

		/// <summary>
		/// Hearthstone process exied
		/// </summary>
		public event Action HearthstoneExited;

		/// <summary>
		/// Hearthstone entered Mode.LOGIN or later for the first time
		/// </summary>
		public event Action HearthstoneLoaded;

		/// <summary>
		/// The log.config has been updated and Hearthstone needs
		/// to be restarted
		/// </summary>
		public event Action HearthstoneRestartRequired;

		/// <summary>
		/// Hearthstone installation directory could not be determined
		/// from the process and log readers could not be started
		/// </summary>
		public event Action HearthstoneInstallationNotFound;

		/// <summary>
		/// The log.config creation or a update to it is required but failed.
		/// </summary>
		public event Action<LogConfigErrorEventArgs> LogConfigError;

		/// <summary>
		/// Hearthstones current mode (i.e. screen) changed
		/// </summary>
		public event Action<ModeChangedEventArgs> ModeChanged;

		/// <summary>
		/// A new match has started but no entities have been created yet.
		/// </summary>
		public event Action<GameCreatedEventArgs> GameCreated;

		/// <summary>
		/// A new match has started.
		/// All necessary data and player entities are available.
		/// </summary>
		public event Action<GameCreatedEventArgs> GameStarted;

		/// <summary>
		/// A match has ended
		/// </summary>
		public event Action<GameEndEventArgs> GameEnded;

		/// <summary>
		/// The gold earned today or the progress towards earning 10 gold changed.
		/// </summary>
		public event Action<GoldProgessWinsEventArgs> GoldProgressWins;

		/// <summary>
		/// A new pack was opened.
		/// </summary>
		public event Action<PackOpenedEventArgs> PackOpened;

		/// <summary>
		/// A friendly challenge has been received.
		/// </summary>
		public event Action FriendlyChallenge;

		/// <summary>
		/// The game queued for a new match.
		/// </summary>
		public event Action<QueuedForGameEventArgs> QueuedForGame;

		/// <summary>
		/// A new dungeon run match started.
		/// </summary>
		public event Action<DungeonRunMatchStartedEventArgs> DungeonRunMatchStarted;

		/// <summary>
		/// The dungeon run deck changed.
		/// </summary>
		public event Action<DungeonRunDeckUpdatedEventArgs> DungeonRunDeckUpdated;

		/// <summary>
		/// The selected deck has been changed.
		/// This deck is not yet set as the in the current/next game.
		/// </summary>
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
		internal virtual void OnGameStarted(GameCreatedEventArgs args) => GameStarted?.Invoke(args);
	}
}
