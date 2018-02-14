using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
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
				[(GameTag.ZONE, Zone.HAND, Zone.PLAY)] = args => CardPlayed?.Invoke(args),
				[(GameTag.ZONE, Zone.DECK, Zone.HAND)] = args => CardDrawn?.Invoke(args),
				[(GameTag.ZONE, Zone.HAND, Zone.DECK)] = args => CardMulliganed?.Invoke(args),
				[(GameTag.ZONE, Zone.HAND, Zone.GRAVEYARD)] = args => CardDiscarded?.Invoke(args),
				[(GameTag.ZONE, Zone.DECK, Zone.GRAVEYARD)] = args => CardMilled?.Invoke(args),
				[(GameTag.ZONE, Zone.PLAY, Zone.HAND)] = args => MinionReturnedToDeck?.Invoke(args),
				[(GameTag.ZONE, Zone.PLAY, Zone.GRAVEYARD)] = args => MinionDied?.Invoke(args),
				[(GameTag.ZONE, Zone.PLAY, Zone.HAND)] = args => MinionReturnedToHand?.Invoke(args),
				[(GameTag.ZONE, Zone.INVALID, Zone.DECK)] = args => CardCreatedInDeck?.Invoke(args),
				[(GameTag.ZONE, Zone.SETASIDE, Zone.DECK)] = args => CardCreatedInDeck?.Invoke(args),
				[(GameTag.ZONE, Zone.INVALID, Zone.HAND)] = args => CardCreatedInHand?.Invoke(args),
				[(GameTag.ZONE, Zone.SETASIDE, Zone.HAND)] = args => CardCreatedInHand?.Invoke(args),
				[(GameTag.ZONE, Zone.INVALID, Zone.PLAY)] = args => CardCreatedInPlay?.Invoke(args),
				[(GameTag.ZONE, Zone.SETASIDE, Zone.PLAY)] = args => CardCreatedInPlay?.Invoke(args),
				[(GameTag.ZONE, Zone.DECK, Zone.PLAY)] = args => CardPlayedFromDeck?.Invoke(args),
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

		public event Action<CommonGameEventArgs> CardDrawn;
		public event Action<CommonGameEventArgs> CardPlayed;
		public event Action<CommonGameEventArgs> CardDiscarded;
		public event Action<CommonGameEventArgs> CardMilled;
		public event Action<CommonGameEventArgs> CardMulliganed;
		public event Action<CommonGameEventArgs> MinionReturnedToDeck;
		public event Action<CommonGameEventArgs> MinionDied;
		public event Action<CommonGameEventArgs> MinionReturnedToHand;
		public event Action<CommonGameEventArgs> CardCreatedInDeck;
		public event Action<CommonGameEventArgs> CardCreatedInHand;
		public event Action<CommonGameEventArgs> CardCreatedInPlay;
		public event Action<CommonGameEventArgs> CardPlayedFromDeck;
		public event Action<AttackEventArgs> Attack;

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
			if(t.Tag == GameTag.DEFENDING)
			{
				var attacker = state.Entities.FirstOrDefault(x => x.Value.HasTag(GameTag.ATTACKING));
				if(t.EntityId.HasValue && attacker.Value != null)
					Attack?.Invoke(new AttackEventArgs(attacker.Value.Id, t.EntityId.Value, state));
			}
		}
	}
}
