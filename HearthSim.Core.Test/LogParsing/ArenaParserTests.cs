using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class ArenaParserTests
	{
		[TestMethod]
		public void RunComplete()
		{
			var complete = false;
			var parser = new ArenaParser();
			parser.ArenaRunComplete += () => complete = true;
			parser.Parse(new Line("Arena", "D 18:21:33.7795321 SetDraftMode - IN_REWARDS"));
			Assert.IsTrue(complete);
		}
	}
}
