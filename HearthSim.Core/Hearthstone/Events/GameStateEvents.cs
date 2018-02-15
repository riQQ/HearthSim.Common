using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone.Events
{
	public class GameStateEvents
	{
		private readonly Dictionary<GameTagChange, Action<EntityGameEvent>> _actions;

		public event Action<EntityGameEvent> CardDrawn;
		public event Action<EntityGameEvent> CardPlayed;
		public event Action<EntityGameEvent> CardDiscarded;
		public event Action<EntityGameEvent> CardMilled;
		public event Action<EntityGameEvent> CardMulliganed;
		public event Action<EntityGameEvent> MinionReturnedToDeck;
		public event Action<EntityGameEvent> MinionDied;
		public event Action<EntityGameEvent> MinionReturnedToHand;
		public event Action<EntityGameEvent> CardCreatedInDeck;
		public event Action<EntityGameEvent> CardCreatedInHand;
		public event Action<EntityGameEvent> CardCreatedInPlay;
		public event Action<EntityGameEvent> CardPlayedFromDeck;
		public event Action<EntityGameEvent> CardStolen;
		public event Action<AttackGameEvent> Attack;
		public event Action<BlockGameEvent> BlockStart;
		public event Action<BlockGameEvent> BlockEnd;
		public event Action<TagChangeGameEvent> TagChange;
		public event Action SetupComplete;
		public event Action<GameStateChangedEventArgs> GameStateChanged;

		public GameStateEvents()
		{
			_actions = new Dictionary<GameTagChange, Action<EntityGameEvent>>
			{
				[(GameTag.ZONE, Zone.HAND, Zone.PLAY)] = args => CardPlayed?.Invoke(args),
				[(GameTag.ZONE, Zone.HAND, Zone.SECRET)] = args => CardPlayed?.Invoke(args),
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
				[(GameTag.ZONE, Zone.INVALID, Zone.SECRET)] = args => CardCreatedInPlay?.Invoke(args),
				[(GameTag.ZONE, Zone.SETASIDE, Zone.PLAY)] = args => CardCreatedInPlay?.Invoke(args),
				[(GameTag.ZONE, Zone.SETASIDE, Zone.SECRET)] = args => CardCreatedInPlay?.Invoke(args),
				[(GameTag.ZONE, Zone.DECK, Zone.PLAY)] = args => CardPlayedFromDeck?.Invoke(args),
				[(GameTag.ZONE, Zone.DECK, Zone.SECRET)] = args => CardPlayedFromDeck?.Invoke(args),
			};
		}

		internal void OnBlockStart(IBlockData block, IGameState gameState) => BlockStart?.Invoke(new BlockGameEvent(block, gameState));

		internal void OnBlockEnd(IBlockData block, IGameState gameState) => BlockEnd?.Invoke(new BlockGameEvent(block, gameState));

		internal void OnSetupComplete() => SetupComplete?.Invoke();

		internal void OnGameStateChanged(GameStateChangedEventArgs args)
		{
			GameStateChanged?.Invoke(args);
			if(args.Modifier is TagChange t)
				OnTagChange(t, args.State);
		}

		private void OnTagChange(TagChange t, IGameState gameState)
		{
			if(!t.EntityId.HasValue)
				return;
			if(_actions.TryGetValue((t.Tag, t.PreviousValue ?? 0, t.Value), out var action))
				action?.Invoke(new EntityGameEvent(t.EntityId.Value, gameState));
			if(t.Tag == GameTag.DEFENDING)
			{
				var attacker = gameState.Entities.FirstOrDefault(x => x.Value.HasTag(GameTag.ATTACKING));
				if(attacker.Value != null)
					Attack?.Invoke(new AttackGameEvent(attacker.Value.Id, t.EntityId.Value, gameState));
			}
			else if(t.Tag == GameTag.CONTROLLER && t.Value > 0 && t.PreviousValue > 0 && t.Value != t.PreviousValue)
				CardStolen?.Invoke(new EntityGameEvent(t.EntityId.Value, gameState));
			TagChange?.Invoke(new TagChangeGameEvent(t, gameState));
		}
	}
}
