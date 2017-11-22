using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public abstract class EntityModifier : IGameStateModifier
	{
		private readonly int _id;

		protected EntityModifier(EntityData data)
		{
			_id = data.Id;
		}

		public void Apply(GameState gameState)
		{
			gameState.CurrentEntity = _id;
			Apply2(gameState);
		}

		public virtual void Apply2(GameState gameState)
		{
		}
	}
}
