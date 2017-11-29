using HearthMirror.Objects;

namespace HearthSim.Core.Hearthstone
{
	public class TavernBrawl
	{
		public int Wins { get; private set; }
		public int Losses { get; private set; }
		public int MaxWins { get; private set; }
		public int MaxLosses { get; private set; }
		public int WinStreak { get; private set; }
		public int GamesPlayed { get; private set; }
		public bool IsSessionBased { get; private set; }

		internal void Unload()
		{
			Wins = 0;
			Losses = 0;
			MaxWins = 0;
			MaxLosses = 0;
			WinStreak = 0;
			GamesPlayed = 0;
			IsSessionBased = false;
		}

		internal void Update(BrawlInfo info)
		{
			Wins = info.Wins;
			Losses = info.Losses;
			MaxWins = info.MaxWins;
			MaxLosses = info.MaxLosses;
			WinStreak = info.WinStreak;
			GamesPlayed = info.GamesPlayed;
			IsSessionBased = info.IsSessionBased;
		}
	}
}
