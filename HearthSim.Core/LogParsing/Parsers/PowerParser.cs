using System;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class PowerParser : ILogParser
	{
		private readonly Regex _blockStartRegex = new Regex(@".*BLOCK_START.*BlockType=(?<type>(\w+)).*id=(?<id>\d*).*(cardId=(?<cardId>(\w*))).*Target=(?<target>(.+)).*SubOption=(?<subOption>(.+))");
		private readonly Regex _creationTagRegex = new Regex(@"tag=(?<tag>(\w+))\ value=(?<value>(\w+))");
		private readonly Regex _hideEntityRegex = new Regex(@"HIDE_ENTITY - .* (id=(?<id>\d+))");
		private readonly Regex _entityRegex = new Regex(@"(?=id=(?<id>(\d+)))(?=name=(?<name>(\w+)))?(?=zone=(?<zone>(\w+)))?(?=zonePos=(?<zonePos>(\d+)))?(?=cardId=(?<cardId>(\w+)))?(?=player=(?<player>(\d+)))?(?=type=(?<type>(\w+)))?");
		private readonly Regex _fullEntityRegex = new Regex(@"FULL_ENTITY - Updating.*id=(?<id>(\d+)).*zone=(?<zone>(\w+)).*CardID=(?<cardId>(\w*))");
		private readonly Regex _gameEntityRegex = new Regex(@"GameEntity\ EntityID=(?<id>(\d+))");
		private readonly Regex _playerEntityRegex = new Regex(@"Player\ EntityID=(?<id>(\d+))\ PlayerID=(?<playerId>(\d+))\ GameAccountId=(?<gameAccountId>(.+))");
		private readonly Regex _tagChangeRegex = new Regex(@"TAG_CHANGE\ Entity=(?<entity>(.+))\ tag=(?<tag>(\w+))\ value=(?<value>(\w+))");
		private readonly Regex _updatingEntityRegex = new Regex(@"(?<type>(SHOW_ENTITY|CHANGE_ENTITY))\ -\ Updating\ Entity=(?<entity>(.+))\ CardID=(?<cardId>(\w*))");
		private readonly Regex _debugDumpRegex = new Regex(@"DebugDump\(\) - ID=(?<id>(\d+) ParentID=\d+ PreviousId=\d+ TaskCount=\d+)");

		private Block _currentBlock;

		public string LogName { get; } = "Power";

		public void Parse(Line line)
		{
			if(line.Text.StartsWith("GameState."))
				HandleGameState(line);
			else
				HandlePowerTaskList(line);
		}

		internal event Action<GameStateLogEventArgs> GameStateLog;
		internal event Action CreateGame;
		internal event Action<BlockData> BlockStart;
		internal event Action BlockEnd;
		internal event Action SetupComplete;

		public event Action<IGameStateModifier> GameStateChange;

		public void HandleGameState(Line line)
		{
			if(line.Text.Contains("CREATE_GAME"))
				CreateGame?.Invoke();
			GameStateLog?.Invoke(new GameStateLogEventArgs(line));
		}

		private EntityData ParseEntity(string entity)
		{
			int id;
			string name = null;
			entity = entity.Replace("UNKNOWN ENTITY ", "");
			var entityMatch = _entityRegex.Match(entity);
			if(entityMatch.Success)
				id = int.Parse(entityMatch.Groups["id"].Value);
			else if(!int.TryParse(entity, out id))
			{
				id = -1;
				name = entity;
			}
			return new EntityData(id, name, null, null);
		}

		private void HandlePowerTaskList(Line line)
		{
			var match = _gameEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var id = int.Parse(match.Groups["id"].Value);
				GameStateChange?.Invoke(new FullEntity(new GameEntityData(id), null));
				return;
			}

			match = _playerEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var entityId = int.Parse(match.Groups["id"].Value);
				var playerId = int.Parse(match.Groups["playerId"].Value);
				GameStateChange?.Invoke(new FullEntity(new PlayerEntityData(entityId, playerId), null));
				return;
			}

			match = _fullEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var id = int.Parse(match.Groups["id"].Value);
				var cardId = match.Groups["cardId"].Value;
				var zone = GameTagParser.ParseEnum<Zone>(match.Groups["zone"].Value);
				if(zone != Zone.SETASIDE)
					cardId = _currentBlock?.Data.NextPredictedCard() ?? cardId;
				GameStateChange?.Invoke(new FullEntity(new EntityData(id, null, cardId, zone), _currentBlock?.Data));
				return;
			}

			match = _tagChangeRegex.Match(line.Text);
			if(match.Success)
			{
				var entity = ParseEntity(match.Groups["entity"].Value);
				Enum.TryParse(match.Groups["tag"].Value, out GameTag tag);
				var value = GameTagParser.ParseTag(tag, match.Groups["value"].Value);
				var entityId = entity.Id == -1 ? null : (int?)entity.Id;
				GameStateChange?.Invoke(new TagChange(new TagChangeData(tag, value, false, entityId, entity.Name)));
				return;
			}

			match = _updatingEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var cardId = match.Groups["cardId"].Value;
				var entity = ParseEntity(match.Groups["entity"].Value);
				var type = match.Groups["type"].Value;
				if(type == "CHANGE_ENTITY")
					GameStateChange?.Invoke(new ChangeEntity(new EntityData(entity.Id, entity.Name, cardId, null)));
				GameStateChange?.Invoke(new ShowEntity(new EntityData(entity.Id, entity.Name, cardId, null)));
				return;
			}

			match = _creationTagRegex.Match(line.Text);
			if(match.Success && !line.Text.Contains("HIDE_ENTITY"))
			{
				var tag = GameTagParser.ParseEnum<GameTag>(match.Groups["tag"].Value);
				var value = GameTagParser.ParseTag(tag, match.Groups["value"].Value);
				GameStateChange?.Invoke(new TagChange(new TagChangeData(tag, value, true, null, null)));
				return;
			}

			match = _hideEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var id = int.Parse(match.Groups["id"].Value);
				GameStateChange?.Invoke(new HideEntity(new EntityData(id, "", null, null)));
			}

			match = _blockStartRegex.Match(line.Text);
			if(match.Success)
			{
				var type = match.Groups["type"].Value;
				var id = int.Parse(match.Groups["id"].Value);
				var cardId = match.Groups["cardId"].Value.Trim();
				var target = match.Groups["target"].Value.Trim();
				var entityMatch = _entityRegex.Match(target);
				EntityData targetData = null;
				if(entityMatch.Success)
				{
					var entityId = int.Parse(entityMatch.Groups["id"].Value.Trim());
					var entityCardId = entityMatch.Groups["cardId"].Value.Trim();
					targetData = new EntityData(entityId, "", entityCardId, null);
				}
				var blockData = new BlockData(type, id, cardId, targetData);
				_currentBlock = _currentBlock?.CreateChild(blockData) ?? new Block(null, blockData);
				BlockStart?.Invoke(blockData);
				return;
			}

			match = _debugDumpRegex.Match(line.Text);
			if(match.Success)
			{
				if(int.Parse(match.Groups["id"].Value) == 2)
					SetupComplete?.Invoke();
			}

			if(line.Text.Contains("BLOCK_END"))
			{
				_currentBlock = _currentBlock?.Parent;
				BlockEnd?.Invoke();
			}
		}

		private class Block
		{
			public Block(Block parent, BlockData data)
			{
				Parent = parent;
				Data = data;
			}

			public Block Parent { get; }
			public BlockData Data { get; }

			public Block CreateChild(BlockData data) => new Block(this, data);
		}
	}
}
