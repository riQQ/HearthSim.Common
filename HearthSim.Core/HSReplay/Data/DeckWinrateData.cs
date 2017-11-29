using System.Collections.Generic;

namespace HearthSim.Core.HSReplay.Data
{
	public class DeckWinrateData : HsReplayData
	{
		public double TotalWinrate { get; set; }
		public Dictionary<string, double> ClassWinrates { get; set; }
	}
}
