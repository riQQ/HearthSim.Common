namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class PlayerEntityData : EntityData
	{
		public int PlayerId { get; }

		public PlayerEntityData(int entityId, int playerId) : base(entityId, null, null, null)
		{
			PlayerId = playerId;
		}
	}
}
