using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.PowerData;

namespace HearthSim.Core.Hearthstone.Entities
{
	public static class EntityFactory
	{
		public static Entity Entity(EntityData data)
		{
			var entity = new Entity(data.Id, data.CardId);
			if(data.Zone.HasValue)
				entity.Tags[GameTag.ZONE] = (int)data.Zone;
			return entity;
		}

		public static PlayerEntity PlayerEntity(PlayerEntityData data)
		{
			return new PlayerEntity(data.Id, data.PlayerId);
		}

		public static GameEntity GameEntity(GameEntityData data)
		{
			return new GameEntity(data.Id);
		}
	}

}
