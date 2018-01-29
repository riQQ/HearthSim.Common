using HearthSim.Core.LogParsing;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class LogParserManagerTests
	{
		[TestMethod]
		public void ParsingTest()
		{
			var manager = new LogParserManager();
			var parser1 = new MockParser("1");
			var parser2 = new MockParser("2");
			var line1 = new Line("1", "Line1");
			var line2 = new Line("2", "Line2");
			var line3 = new Line("1", "Line3");

			manager.RegisterParser(parser1);
			manager.RegisterParser(parser2);

			manager.Parse(new [] {line1});
			Assert.AreEqual(line1, parser1.LastParsedLine);
			Assert.IsNull(parser2.LastParsedLine);

			manager.Parse(new [] {line2});
			Assert.AreEqual(line1, parser1.LastParsedLine);
			Assert.AreEqual(line2, parser2.LastParsedLine);

			manager.UnregisterParser(parser1);
			manager.Parse(new[] {line3});
			Assert.AreEqual(line1, parser1.LastParsedLine);

			manager.RegisterParser(parser1);
			manager.Parse(new[] {line3});
			Assert.AreEqual(line3, parser1.LastParsedLine);
		}
	}
}
