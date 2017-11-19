using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.PowerData;

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
			if(CreationTag)
				EntityId = gameState.CurrentEntity;
			if(!EntityId.HasValue)
				return;
			var entity = gameState.Entities[EntityId.Value];
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
