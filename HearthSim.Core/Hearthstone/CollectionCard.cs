namespace HearthSim.Core.Hearthstone
{
	public class CollectionCard
	{
		public string Id { get; }
		public int Normal { get; }
		public int Golden { get; }

		public CollectionCard(string id, int normal, int golden)
		{
			Id = id;
			Normal = normal;
			Golden = golden;
		}

		public Card ToCard() => new Card(this);
	}
}
