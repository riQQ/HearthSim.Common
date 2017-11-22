using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class FullEntity : EntityModifier
	{
		private readonly EntityData _data;

		public FullEntity(EntityData data) : base(data)
		{
			_data = data;
		}

		public override void Apply2(GameState gameState)
		{
			if(_data is GameEntityData g)
			{
				var entity = EntityFactory.GameEntity(g);
				gameState.GameEntity = entity;
				gameState.Entities[_data.Id] = gameState.GameEntity;
			}
			else if(_data is PlayerEntityData p)
			{
				var entity = EntityFactory.PlayerEntity(p);
				gameState.PlayerEntities[entity.PlayerId] = entity;
				gameState.Entities[_data.Id] = entity;
			}
			else
				gameState.Entities[_data.Id] = EntityFactory.Entity(_data);
		}

		public override string ToString()
		{
			return $"FULL_ENTITY Id={_data.Id} Name={_data.Name} Zone={_data.Zone} CardId={_data.CardId}";
		}
	}
}
