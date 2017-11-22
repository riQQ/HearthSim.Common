using HearthDb.Enums;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class EntityData
	{
		public EntityData(int id, string name, string cardId, Zone? zone)
		{
			Id = id;
			Name = name;
			CardId = cardId;
			Zone = zone;
		}

		public int Id { get; }
		public string Name { get; }
		public string CardId { get; }
		public Zone? Zone { get; }
	}
}
