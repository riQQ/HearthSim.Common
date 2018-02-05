using System;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;

namespace HearthSim.Core.Hearthstone
{
	public static class GameStateChangeHelper
	{
		public static Action<IGameStateModifier, IGameState> OnGameEnd(Action action)
		{
			return OnTagChange(GameTag.STATE, (int)State.COMPLETE, entity => action());
		}

		private static Action<IGameStateModifier, IGameState> OnTagChange(GameTag tag, int value, Action<Entity> action)
		{
			return OnTagChange(tag, null, value, action);
		}

		private static Action<IGameStateModifier, IGameState> OnTagChange(GameTag tag, int? prev, int value, Action<Entity> action)
		{
			return (modifier, state) =>
			{
				if(!(modifier is TagChange tagChange) || !tagChange.EntityId.HasValue)
					return;
				if(tagChange.Tag == tag && tagChange.Value == value && (!prev.HasValue || prev.Value == tagChange.PreviousValue))
				{
					if(state.Entities.TryGetValue(tagChange.EntityId.Value, out var entity))
						action.Invoke(entity);
				}
			};
		}

		public static Action<IGameStateModifier, IGameState> OnCardPlayed(Action<Entity> action)
		{
			return OnTagChange(GameTag.ZONE, (int)Zone.HAND, (int)Zone.PLAY, action);
		}

		public static Action<IGameStateModifier, IGameState> OnCardDrawn(Action<Entity> action)
		{
			return OnTagChange(GameTag.ZONE, (int)Zone.DECK, (int)Zone.HAND, action);
		}
	}
}
