using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Test.MockData
{
	public class MockGameInfo : IGameInfoProvider
	{
		public string LastPlayedCard { get; set; }
	}
}
