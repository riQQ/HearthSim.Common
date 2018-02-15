using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;

namespace HearthSim.Core.Test.Secrets
{
	public class EntityHelper
	{
		private readonly IGameState _gameState;

		public EntityHelper(IGameState gameState)
		{
			_gameState = gameState;
		}

		public Entity Create(PlayerEntity controller, string cardId, Zone zone = Zone.INVALID)
		{
			var id = _gameState.Entities.Keys.Max() + 1;
			var entity = new Entity(id, cardId);
			foreach(var tag in entity.Card.Data.Entity.Tags)
				entity.SetTag((GameTag)tag.EnumId, tag.Value);
			entity.SetTag(GameTag.CONTROLLER, controller.PlayerId);
			if(zone != Zone.INVALID)
			{
				entity.SetTag(GameTag.ZONE, (int)zone);
				var zonePos = _gameState.Entities.Values.Count(x => x.IsControlledBy(controller.PlayerId) && x.IsInZone(zone) && x.IsMinion) + 1;
				entity.SetTag(GameTag.ZONE_POSITION, zonePos);
			}
			_gameState.Entities[id] = entity;
			return entity;
		}

		public PlayerEntity CreatePlayer()
		{
			var id = _gameState.Entities.Keys.DefaultIfEmpty(0).Max() + 1;
			var playerId = _gameState.PlayerEntities.Keys.DefaultIfEmpty(1).Max() + 1;
			var entity = new PlayerEntity(id, playerId);
			_gameState.Entities[id] = entity;
			_gameState.PlayerEntities[playerId] = entity;
			return entity;
		}

		public GameEntity CreateGame()
		{
			var entity = new GameEntity(1);
			_gameState.Entities[1] = entity;
			_gameState.GameEntity = entity;
			return entity;
		}
	}
}
