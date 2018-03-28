﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.Events;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Hearthstone.Secrets;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Logging;

namespace HearthSim.Core.Hearthstone
{
	public class GameState : IGameState
	{
		private readonly IGameDataProvider _gameDataProvider;
		private readonly Queue<IGameStateModifier> _creationTags;
		private readonly List<IGameStateModifier> _modifiers;

		private MatchInfo _matchInfo;
		private readonly List<string> _powerLog;
		private GameServerInfo _serverInfo;

		public GameState(IGameDataProvider gameDataProvider, GameStateEvents gameStateEvents)
		{
			gameDataProvider.Reset();
			_gameDataProvider = gameDataProvider;
			Entities = new Dictionary<int, Entity>();
			PlayerEntities = new Dictionary<int, PlayerEntity>();
			_modifiers = new List<IGameStateModifier>();
			_creationTags = new Queue<IGameStateModifier>();
			_powerLog = new List<string>();
			CreatedAt = DateTime.Now;
			Modified += gameStateEvents.OnGameStateChanged;
			SecretsManager = new SecretsManager(this, gameStateEvents);
			LocalPlayer = new Player(this, true);
			OpposingPlayer = new Player(this, false);
			GameTime = new GameTime();
			Task.Run(async () =>
			{
				while(MatchInfo == null || ServerInfo == null)
					await Task.Delay(500);
			});
		}

		public IReadOnlyCollection<string> PowerLog => _powerLog.AsReadOnly();
		public MatchInfo MatchInfo => _matchInfo ?? (_matchInfo = _gameDataProvider.GetMatchInfo());
		public GameServerInfo ServerInfo => _serverInfo ?? (_serverInfo = _gameDataProvider.GetServerInfo());

		public bool SetupComplete { get; internal set; }
		public Dictionary<int, Entity> Entities { get; }
		public GameEntity GameEntity { get; set; }
		public Dictionary<int, PlayerEntity> PlayerEntities { get; }

		public PlayerEntity LocalPlayerEntity => TryGetPlayerEntity(MatchInfo?.LocalPlayer);
		public PlayerEntity OpposingPlayerEntity => TryGetPlayerEntity(MatchInfo?.OpposingPlayer);
		public PlayerEntity CurrentPlayerEntity => PlayerEntities.Values.FirstOrDefault(x => x.HasTag(GameTag.CURRENT_PLAYER));

		public Player LocalPlayer { get; }
		public Player OpposingPlayer { get; }
		public Player CurrentPlayer => GetPlayer(CurrentPlayerEntity?.PlayerId ?? 0);

		public Player GetPlayer(int playerId) =>
			new[] { LocalPlayer, OpposingPlayer }.FirstOrDefault(x => x.PlayerId == playerId);

		internal SecretsManager SecretsManager { get; }

		public GameTime GameTime { get; }

		public Entity LastCardPlayed 
			=> Entities.TryGetValue(GameEntity.GetTag(GameTag.LAST_CARD_PLAYED), out var entity) ? entity : null;

		public DateTime CreatedAt { get; }

		internal event Action<GameStateChangedEventArgs> Modified;

		private PlayerEntity TryGetPlayerEntity(MatchInfo.Player player)
			=> player != null && PlayerEntities.TryGetValue(player.Id, out var playerEntity) ? playerEntity : null;

		public bool IsMulliganDone => LocalPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE
									&& OpposingPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE;

		public int CurrentTurn => !IsMulliganDone ? 0 : ((GameEntity?.GetTag(GameTag.TURN) + 1) / 2 ?? 0);

		internal void Apply(IGameStateModifier modifier)
		{
			var tagChange = modifier as TagChange;
			if(tagChange != null && !tagChange.CanApply)
			{
				if(TryResolveEntityName(tagChange.EntityName, out var entityId))
					tagChange.ResolveEntityId(entityId);
				else
				{
					Log.Debug($"Could not apply tag={tagChange.Tag}, value={tagChange.Value} on {tagChange.EntityName}");
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

			if(tagChange?.Tag == GameTag.ZONE && tagChange.EntityId.HasValue
				&& Entities.TryGetValue(tagChange.EntityId.Value, out var entity))
			{
				if(entity.IsSpell && tagChange.Value == (int)Zone.PLAY && tagChange.PreviousValue == (int)Zone.HAND)
				{
					var controller = GetPlayer(entity.Controller);
					if(controller != null)
						controller.SpellsPlayed++;
				}
			}

			if(!isCreationTag)
				Modified?.Invoke(new GameStateChangedEventArgs(modifier, this));
		}

		private bool TryResolveEntityName(string name, out int entityId, bool retry = true)
		{
			entityId = -1;
			if(name == null)
				return false;
			if(name == "GameEntity")
			{
				entityId = GameEntity.Id;
				return true;
			}
			var matchInfo = MatchInfo;
			if(matchInfo == null)
				return false;

			bool IsPlayer(MatchInfo.Player player)
				=> player.BattleTag != null
					&& ($"{player.BattleTag.Name}#{player.BattleTag.Number}" == name
						|| player.BattleTag.Name == name)
					|| player.Name == name;

			if(IsPlayer(matchInfo.LocalPlayer))
			{
				entityId = PlayerEntities[matchInfo.LocalPlayer.Id].Id;
				return true;
			}
			if(IsPlayer(matchInfo.OpposingPlayer))
			{
				entityId = PlayerEntities[matchInfo.OpposingPlayer.Id].Id;
				return true;
			}

			if(retry)
			{
				Log.Debug($"Could not resolve entity \"{name}\", retrying");
				_matchInfo = null;
				_gameDataProvider.Reset();
				return TryResolveEntityName(name, out entityId, false);
			}
			return false;
		}

		public void AppendLog(LogEventArgs args) => _powerLog.Add(args.Line.RawLine);
	}
}
