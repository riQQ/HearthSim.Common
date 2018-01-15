using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class FullEntity : EntityModifier
	{
		private readonly EntityData _data;
		private readonly bool _joustReveal;

		public FullEntity(EntityData data, BlockData parentBlock) : base(data)
		{
			_data = data;
			_joustReveal = parentBlock?.Type == BlockType.JOUST;
		}

		public override void Apply2(GameState gameState)
		{
			if(_data is GameEntityData g)
			{
				var entity = new GameEntity(g.Id);
				gameState.GameEntity = entity;
				gameState.Entities[_data.Id] = gameState.GameEntity;
			}
			else if(_data is PlayerEntityData p)
			{
				var entity = new PlayerEntity(p.Id, p.PlayerId);
				gameState.PlayerEntities[entity.PlayerId] = entity;
				gameState.Entities[_data.Id] = entity;
			}
			else
			{
				var entity = new Entity(_data.Id, _data.CardId);
				if(_data.Zone.HasValue)
				{
					entity.SetTag(GameTag.ZONE, (int)_data.Zone.Value);
					entity.Info.OriginalZone = _data.Zone.Value;
					if(gameState.IsMulliganDone && gameState.SetupComplete)
						entity.Info.IsCreated = true;
				}
				entity.Info.JoustReveal = _joustReveal;
				gameState.Entities[_data.Id] = entity;
			}
		}

		public override string ToString()
		{
			return $"FULL_ENTITY Id={_data.Id} Name={_data.Name} Zone={_data.Zone} CardId={_data.CardId}";
		}
	}
}
