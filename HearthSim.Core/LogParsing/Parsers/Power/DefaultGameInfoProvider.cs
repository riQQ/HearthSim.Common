using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class DefaultGameInfoProvider : IGameInfoProvider
	{
		private readonly Game _game;

		public DefaultGameInfoProvider(Game game)
		{
			_game = game;
		}

		public string LastPlayedCard => _game?.CurrentGame?.LastCardPlayed?.CardId;
	}
}
