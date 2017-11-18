namespace HearthSim.Core.LogParsing.Parsers.PowerData
{
	public class BlockData
	{
		public BlockData(string type, int id, string cardId)
		{
			Type = type;
			Id = id;
			CardId = cardId;
		}

		public string Type { get; }
		public int Id { get; }
		public string CardId { get; }
	}
}
