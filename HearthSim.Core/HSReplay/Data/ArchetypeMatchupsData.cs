using System.Collections.Generic;

namespace HearthSim.Core.HSReplay.Data
{
	public class ArchetypeMatchupsData : HsReplayData
	{
		public Dictionary<string, Dictionary<string, double>> ArchetypeMatchups { get; set; }
	}
}
