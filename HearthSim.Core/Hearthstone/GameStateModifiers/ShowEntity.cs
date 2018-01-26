using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class ShowEntity : EntityModifier
	{
		public int EntityId { get; }
		public string CardId { get; }
		public IBlockData ParentBlock { get; }

		public ShowEntity(EntityData data, IBlockData parentBlock) : base(data)
		{
			EntityId = data.Id;
			CardId = data.CardId;
			ParentBlock = parentBlock;
		}

		protected override void ApplyImpl(GameState gameState)
		{
			if(gameState.Entities.TryGetValue(EntityId, out var entity))
			{
				entity.CardId = CardId;
				entity.Info.Hidden = false;
				entity.Info.JoustReveal = ParentBlock?.Type == BlockType.REVEAL_CARD;
			}
		}

		public override string ToString()
		{
			return $"SHOW_ENTITY Id={EntityId} CardId={CardId}";
		}
	}
}
