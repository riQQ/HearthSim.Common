using HearthDb.Enums;

namespace HearthSim.Core.LogParsing
{
	public class PowerAction
	{
		public PowerAction(PowerType type)
		{
			Type = type;
		}

		public PowerType Type { get; }
	}
}
