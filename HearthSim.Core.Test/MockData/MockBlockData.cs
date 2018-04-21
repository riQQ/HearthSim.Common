using HearthDb.Enums;
using HearthSim.Core.LogParsing.Parsers.Power;

namespace HearthSim.Core.Test.MockData
{
	public class MockBlockData : IBlockData
	{
		public string NextPredictedCard()
		{
			return null;
		}

		public void AddPredictedCard(string cardId)
		{
		}

		public EntityData Target { get; set; }
		public string CardId { get; set; }
		public BlockType? Type { get; set; }
		public int Id { get; set; }
		public string EffectCardId { get; set; }
		public int EffectIndex { get; set; }
	}
}
