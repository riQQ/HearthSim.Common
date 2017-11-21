using System;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.EventArgs;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogParsing.Parsers.PowerData;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class PowerParser : ILogParser
	{
		private readonly Regex _blockStartRegex =
			new Regex(@".*BLOCK_START.*BlockType=(?<type>(\w+)).*id=(?<id>\d*).*(cardId=(?<Id>(\w*))).*Target=(?<target>(.+)).*SubOption=(?<subOption>(.+))");

		private readonly Regex _fullEntityRegex = new Regex(@"FULL_ENTITY - Updating.*id=(?<id>(\d+)).*zone=(?<zone>(\w+)).*CardID=(?<cardId>(\w*))");
		private readonly Regex _creationTagRegex = new Regex(@"tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

		private readonly Regex _entityRegex =
			new Regex(
				@"(?=id=(?<id>(\d+)))(?=name=(?<name>(\w+)))?(?=zone=(?<zone>(\w+)))?(?=zonePos=(?<zonePos>(\d+)))?(?=cardId=(?<cardId>(\w+)))?(?=player=(?<player>(\d+)))?(?=type=(?<type>(\w+)))?");

		private readonly Regex _gameEntityRegex = new Regex(@"GameEntity\ EntityID=(?<id>(\d+))");

		private readonly Regex _playerEntityRegex =
			new Regex(@"Player\ EntityID=(?<id>(\d+))\ PlayerID=(?<playerId>(\d+))\ GameAccountId=(?<gameAccountId>(.+))");

		private readonly Regex _tagChangeRegex =
			new Regex(@"TAG_CHANGE\ Entity=(?<entity>(.+))\ tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

		private readonly Regex _updatingEntityRegex =
			new Regex(@"(?<type>(SHOW_ENTITY|CHANGE_ENTITY))\ -\ Updating\ Entity=(?<entity>(.+))\ CardID=(?<cardId>(\w*))");

		public string LogName { get; } = "Power";

		public event Action<GameStateLogEventArgs> GameStateLog;
		public event Action CreateGame;
		public event Action<BlockData> BlockStart;
		public event Action StartSpectator;
		public event Action EndSpectator;
		public event Action BlockEnd;

		public event Action<IGameStateModifier> GameStateChange;

		public void Parse(Line line)
		{
			if(line.Text.StartsWith("GameState."))
				GameStateLog?.Invoke(new GameStateLogEventArgs(line));
			else
				HandlePowerTaskList(line);
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
			if(line.Text.Contains("CREATE_GAME"))
			{
				CreateGame?.Invoke();
				return;
			}

			var match = _gameEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var id = int.Parse(match.Groups["id"].Value);
				GameStateChange?.Invoke(new FullEntity(new GameEntityData(id)));
				return;
			}

			match = _playerEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var entityId = int.Parse(match.Groups["id"].Value);
				var playerId = int.Parse(match.Groups["playerId"].Value);
				GameStateChange?.Invoke(new FullEntity(new PlayerEntityData(entityId, playerId)));
				return;
			}

			match = _fullEntityRegex.Match(line.Text);
			if(match.Success)
			{
				var id = int.Parse(match.Groups["id"].Value);
				var cardId = match.Groups["cardId"].Value;
				var zone = GameTagParser.ParseEnum<Zone>(match.Groups["zone"].Value);
				GameStateChange?.Invoke(new FullEntity(new EntityData(id, null, cardId, zone)));
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

			if(line.Text.Contains("End Spectator"))
			{
				EndSpectator?.Invoke();
				return;
			}

			match = _blockStartRegex.Match(line.Text);
			if(match.Success)
			{
				var type = match.Groups["type"].Value;
				var id = int.Parse(match.Groups["id"].Value);
				var cardId = match.Groups["cardId"].Value.Trim();
				BlockStart?.Invoke(new BlockData(type, id, cardId));
				return;
			}

			if(line.Text.Contains("BLOCK_END"))
			{
				BlockEnd?.Invoke();
				return;
			}
		}
	}
}
