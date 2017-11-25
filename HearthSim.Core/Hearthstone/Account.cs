using System;

namespace HearthSim.Core.Hearthstone
{
	public class Account
	{
		public ulong AccountHi { get; private set; }
		public ulong AccountLo { get; private set; }
		public string BattleTagName { get; private set; }
		public int BattleTagNumber { get; private set; }
		public bool IsLoaded { get; set; }

		public string BattleTag => $"{BattleTagName}#{BattleTagNumber}";

		public event Action Loaded;

		internal void Update(ulong accountHi, ulong accountLo, string battleTagName, int battleTagNumber)
		{
			AccountHi = accountHi;
			AccountLo = accountLo;
			BattleTagName = battleTagName;
			BattleTagNumber = battleTagNumber;
			Loaded?.Invoke();
			IsLoaded = true;
		}

		internal void Unload()
		{
			AccountHi = 0;
			AccountLo = 0;
			BattleTagName = string.Empty;
			BattleTagNumber = 0;
			IsLoaded = false;
		}
	}
}
