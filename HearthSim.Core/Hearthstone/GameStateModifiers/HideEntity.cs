using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public class HideEntity : EntityModifier
	{
		private readonly int _entityId;

		public HideEntity(EntityData data) : base(data)
		{
			_entityId = data.Id;
		}

		public override void Apply2(GameState gameState)
		{
			gameState.Entities[_entityId].Info.Hidden = true;
		}
	}
}
