using System;

namespace HearthSim.Core.HSReplay
{
	public class BlizzardAccountClaimEventArgs : EventArgs
	{
		public ulong Hi { get; }
		public ulong Lo { get; }
		public string BattleTag { get; }
		public ClaimError Error { get; }

		public BlizzardAccountClaimEventArgs(ulong hi, ulong lo, string battleTag)
			: this(ClaimError.None, hi, lo, battleTag)
		{
		}

		public BlizzardAccountClaimEventArgs(ClaimError error, ulong hi, ulong lo, string battleTag)
		{
			Error = error;
			Hi = hi;
			Lo = lo;
			BattleTag = battleTag;
		}
	}
}