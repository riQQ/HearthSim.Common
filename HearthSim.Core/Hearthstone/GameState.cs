using System;
using System.Collections.Generic;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Hearthstone
{
	public class GameState
	{
		private readonly Queue<IGameStateModifier> _creationTags;
		private readonly List<IGameStateModifier> _modifiers;

		private MatchInfo _matchInfo;
		private readonly List<string> _powerLog;

		public GameState()
		{
			Entities = new Dictionary<int, Entity>();
			PlayerEntities = new Dictionary<int, PlayerEntity>();
			_modifiers = new List<IGameStateModifier>();
			_creationTags = new Queue<IGameStateModifier>();
			LocalPlayer = new Player(this, true);
			OpposingPlayer = new Player(this, false);
			_powerLog = new List<string>();
		}

		public IReadOnlyCollection<string> PowerLog => _powerLog.AsReadOnly();
		public MatchInfo MatchInfo => _matchInfo ?? (_matchInfo = Reflection.GetMatchInfo());

		public Dictionary<int, Entity> Entities { get; }
		public GameEntity GameEntity { get; set; }
		public Dictionary<int, PlayerEntity> PlayerEntities { get; }

		public PlayerEntity LocalPlayerEntity => TryGetPlayerEntity(MatchInfo?.LocalPlayer);
		public PlayerEntity OpposingPlayerEntity => TryGetPlayerEntity(MatchInfo?.OpposingPlayer);

		public Player LocalPlayer { get; }
		public Player OpposingPlayer { get; }

		//TODO move this into the parser
		public int CurrentEntity { get; internal set; }

		public Entity LastCardPlayed 
			=> Entities.TryGetValue(GameEntity.GetTag(GameTag.LAST_CARD_PLAYED), out var entity) ? entity : null;

		internal event Action<GameStateChangedEventArgs> Modified;

		private PlayerEntity TryGetPlayerEntity(MatchInfo.Player player)
			=> player != null && PlayerEntities.TryGetValue(player.Id, out var playerEntity) ? playerEntity : null;

		internal void Apply(IGameStateModifier modifier)
		{
			var tagChange = modifier as TagChange;
			if(tagChange != null && !tagChange.CanApply)
			{
				if(TryResolveEntityName(tagChange.EntityName, out var entityId))
					tagChange.ResolveEntityId(entityId);
				else
				{
					Log.Debug($"Could not apply tag={tagChange.Tag}, value={tagChange.Tag} on {tagChange.EntityName}");
					return;
				}
			}

			var isCreationTag = tagChange?.CreationTag == true;

			// Wait for all creation tags to be applied before invoking Modified
			if(isCreationTag)
				_creationTags.Enqueue(tagChange);
			else
			{
				while(_creationTags.Count > 0)
					Modified?.Invoke(new GameStateChangedEventArgs(_creationTags.Dequeue(), this));
			}

			modifier.Apply(this);
			_modifiers.Add(modifier);

			if(!isCreationTag)
				Modified?.Invoke(new GameStateChangedEventArgs(modifier, this));
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

		public void AppendLog(GameStateLogEventArgs args) => _powerLog.Add(args.Line.RawLine);
	}
}
