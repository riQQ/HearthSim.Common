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

		public void Apply(IGameState gameState)
		{
			gameState.CurrentEntity = _id;
			ApplyImpl(gameState);
		}

		protected abstract void ApplyImpl(IGameState gameState);
	}
}
