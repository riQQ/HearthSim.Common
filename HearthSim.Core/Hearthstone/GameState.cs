using System;
using System.Collections.Generic;
using HearthMirror;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Hearthstone
{
	public class GameState
	{
		private readonly List<IGameStateModifier> _modifiers;
		private readonly List<IGameStateModifier> _outstandingModifications;

		private MatchInfo _matchInfo;

		public GameState()
		{
			Entities = new Dictionary<int, Entity>();
			PlayerEntities = new Dictionary<int, PlayerEntity>();
			_modifiers = new List<IGameStateModifier>();
			_outstandingModifications = new List<IGameStateModifier>();
			LocalPlayer = new Player(this, true);
			OpposingPlayer = new Player(this, false);
		}

		public MatchInfo MatchInfo => _matchInfo ?? (_matchInfo = Reflection.GetMatchInfo());

		public Dictionary<int, Entity> Entities { get; }
		public GameEntity GameEntity { get; set; }
		public Dictionary<int, PlayerEntity> PlayerEntities { get; }

		public PlayerEntity LocalPlayerEntity => PlayerEntities.TryGetValue(MatchInfo?.LocalPlayer.Id ?? -1, out var player) ? player : null;
		public PlayerEntity OpposingPlayerEntity => PlayerEntities.TryGetValue(MatchInfo?.OpposingPlayer.Id ?? -1, out var player) ? player : null;

		public Player LocalPlayer { get; }
		public Player OpposingPlayer { get; }

		public int CurrentEntity { get; internal set; }

		internal void Apply(IGameStateModifier modifier)
		{
			if(modifier is TagChange tagChange && !tagChange.CanApply)
			{
				if(TryResolveEntityName(tagChange.EntityName, out var entityId))
					tagChange.ResolveEntityId(entityId);
				else
				{
					Log.Debug($"Could not apply tag={tagChange.Tag}, value={tagChange.Tag} on {tagChange.EntityName}");
					_outstandingModifications.Add(modifier);
					return;
				}
			}
			modifier.Apply(this);
			_modifiers.Add(modifier);
		}

		private bool TryResolveEntityName(string name, out int entityId)
		{
			entityId = -1;
			if(name == null)
				return false;
			if(name == "GameEntity")
			{
				entityId = GameEntity.Id;
				return true;
			}
			if(MatchInfo?.LocalPlayer.Name == name)
			{
				entityId = PlayerEntities[MatchInfo.LocalPlayer.Id].Id;
				return true;
			}
			if(MatchInfo?.OpposingPlayer.Name == name)
			{
				entityId = PlayerEntities[MatchInfo.OpposingPlayer.Id].Id;
				return true;
			}
			return false;
		}
	}
}
