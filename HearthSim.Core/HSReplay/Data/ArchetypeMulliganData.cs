using System.Collections.Generic;
using Newtonsoft.Json;

namespace HearthSim.Core.HSReplay.Data
{
	public class ArchetypeMulliganData : HsReplayData
	{
		public List<MulliganData> MulliganData { get; set; }
	}

	public class MulliganData
	{
		[JsonProperty("dbf_id")]
		public int DbfId { get; set; }

		[JsonProperty("winrate_when_drawn")]
		public double? DrawnWinrate { get; set; }

		[JsonProperty("opening_hand_winrate")]
		public double? MulliganWinrate { get; set; }

		[JsonProperty("winrate_when_played")]
		public double? PlayedWinrate { get; set; }

		[JsonProperty("keep_percentage")]
		public double? Kept { get; set; }

		[JsonProperty("times_drawn")]
		public int? TimesDrawn { get; set; }

		[JsonProperty("times_in_presented_in_initial_cards")]
		public int? TimesInInitialCards { get; set; }

		[JsonProperty("times_in_opening_hand")]
		public int? TimesInOpeningHand { get; set; }

		[JsonProperty("times_kept")]
		public int? TimesKept { get; set; }

		[JsonProperty("times_played")]
		public int? TimesPlayed { get; set; }

		[JsonProperty("avg_turn_played_on")]
		public double? TurnPlayed { get; set; }

		[JsonProperty("avg_turns_in_hand")]
		public double? TurnsInHand { get; set; }
	}
}
