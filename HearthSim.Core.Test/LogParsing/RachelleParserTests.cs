using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class RachelleParserTests
	{
		[TestMethod]
		public void GoldProgress()
		{
			var wins = 0;
			var parser = new RachelleParser();
			parser.GoldProgressWins += args => wins = args.Wins;
			parser.Parse(new Line("Rachelle", "D 13:46:22.9119997 EndGameTwoScoop.UpdateData(): 2/3 wins towards 10 gold"));
			Assert.AreEqual(2, wins);
		}

		[TestMethod]
		public void DeckDeleted()
		{
			var deckId = 0L;
			var parser = new RachelleParser();
			parser.DeckDeleted += args => deckId = args.DeckId;
			parser.Parse(new Line("Rachelle", "D 17:18:37.3435108 DeckDeleted:1473595360"));
			Assert.AreEqual(1473595360, deckId);
		}
	}
}
