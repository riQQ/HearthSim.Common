namespace HearthSim.Core.Hearthstone.GameStateModifiers
{
	public interface IGameStateModifier
	{
		void Apply(IGameState gameState);
	}
}
