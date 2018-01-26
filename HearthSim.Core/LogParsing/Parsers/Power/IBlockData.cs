using HearthDb.Enums;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public interface IBlockData
	{
		string NextPredictedCard();
		void AddPredictedCard(string cardId);
		EntityData Target { get; }
		string CardId { get; }
		BlockType? Type { get; }
		int Id { get; }
	}
}
