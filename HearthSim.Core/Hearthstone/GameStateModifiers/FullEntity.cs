using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class FullEntity : EntityModifier
	{
		public EntityData Data { get; }
		public IBlockData ParentBlock { get; }

		public FullEntity(EntityData data, IBlockData parentBlock) : base(data)
		{
			Data = data;
			ParentBlock = parentBlock;
		}

		protected override void ApplyImpl(GameState gameState)
		{
			if(Data is GameEntityData g)
			{
				var entity = new GameEntity(g.Id);
				gameState.GameEntity = entity;
				gameState.Entities[Data.Id] = gameState.GameEntity;
			}
			else if(Data is PlayerEntityData p)
			{
				var entity = new PlayerEntity(p.Id, p.PlayerId);
				gameState.PlayerEntities[entity.PlayerId] = entity;
				gameState.Entities[Data.Id] = entity;
			}
			else
			{
				var entity = new Entity(Data.Id, Data.CardId);
				if(Data.Zone.HasValue)
				{
					entity.SetTag(GameTag.ZONE, (int)Data.Zone.Value);
					entity.Info.OriginalZone = Data.Zone.Value;
				}
				if(gameState.IsMulliganDone && gameState.SetupComplete)
					entity.Info.IsCreated = true;
				entity.Info.JoustReveal = ParentBlock?.Type == BlockType.JOUST;
				gameState.Entities[Data.Id] = entity;
			}
		}

		public override string ToString()
		{
			return $"FULL_ENTITY Id={Data.Id} Name={Data.Name} Zone={Data.Zone} CardId={Data.CardId}";
		}
	}
}
