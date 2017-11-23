using System;
using System.Text.RegularExpressions;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class RachelleParser : ILogParser
	{
		private readonly Regex _deckDeletedRegex = new Regex(@"DeckDeleted:\s*(?<deckId>(\d+))");
		private readonly Regex _goldProgressRegex = new Regex(@"(?<wins>(\d))/3 wins towards 10 gold");

		public string LogName { get; } = "Rachelle";

		public void Parse(Line line)
		{
			var match = _goldProgressRegex.Match(line.Text);
			if(match.Success)
			{
				var wins = int.Parse(match.Groups["wins"].Value);
				GoldProgressWins?.Invoke(wins);
			}

			match = _deckDeletedRegex.Match(line.Text);
			if(match.Success)
			{
				var deckId = long.Parse(match.Groups["deckId"].Value);
				DeckDeleted?.Invoke(deckId);
			}
		}

		public event Action<int> GoldProgressWins;
		public event Action<long> DeckDeleted;
	}
}