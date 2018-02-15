using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Hearthstone.Events
{
	public class BlockGameEvent : BaseGameEvent<IBlockData>
	{
		public BlockGameEvent(IBlockData data, IGameState state) : base(data, state)
		{
		}
	}
}
