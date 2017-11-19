using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.PowerData;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class TagChange : IGameStateModifier
	{
		public int Value { get; }
		public GameTag Tag { get; }
		public int? EntityId { get; private set; }
		public int? PreviousValue { get; private set; }
		public string EntityName { get; }
		public bool CreationTag { get; set; }

		public TagChange(TagChangeData data)
		{
			EntityId = data.EntityId;
			Tag = data.Tag;
			Value = data.Value;
			EntityName = data.EntityName;
			CreationTag = data.CreationTag;
		}

		public void Apply(GameState gameState)
		{
			if(!CreationTag && !EntityId.HasValue)
				return;
			var entityId = CreationTag ? gameState.CurrentEntity : EntityId.Value;
			var entity = gameState.Entities[entityId];
			PreviousValue = entity.GetTag(Tag);
			entity.Tags[Tag] = Value;
		}

		public bool CanApply => CreationTag || EntityId.HasValue;

		public void ResolveEntityId(int entityId)
		{
			if(!EntityId.HasValue)
				EntityId = entityId;
		}

		public override string ToString()
		{
			return $"{(CreationTag ? "\t" : "")}TAG_CHANGE Entity={EntityId?.ToString() ?? EntityName} Tag={Tag} Value={Value} Previous={PreviousValue}";
		}
	}
}
