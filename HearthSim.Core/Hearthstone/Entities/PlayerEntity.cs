namespace HearthSim.Core.Hearthstone.Entities
{
	public class PlayerEntity : Entity
	{
		public int PlayerId { get; }

		public PlayerEntity(int id, int playerId) : base(id, null)
		{
			PlayerId = playerId;
		}
	}

}
