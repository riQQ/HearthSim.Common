using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.EventArgs
{
	public class ArenaRunCompleteEventArgs : System.EventArgs
	{
		public List<RewardData> Rewards { get; }

		public ArenaRunCompleteEventArgs(List<RewardData> rewards)
		{
			Rewards = rewards;
		}
	}
}
