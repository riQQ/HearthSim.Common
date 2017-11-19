using System.Collections.Generic;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone.Entities
{
	public class Entity
	{
		public Entity(int id, string cardId)
		{
			Id = id;
			CardId = cardId;
			Tags = new Dictionary<GameTag, int>();
		}

		public int Id { get; }
		public string CardId { get; set; }
		public Dictionary<GameTag, int> Tags { get; }

		public int GetTag(GameTag tag)
		{
			Tags.TryGetValue(tag, out var value);
			return value;
		}

		public bool HasTag(GameTag tag)
		{
			return GetTag(tag) > 0;
		}
	}
}
