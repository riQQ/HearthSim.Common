using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.Test.MockData
{
	public class MockParser : ILogParser
	{
		public MockParser(string logName)
		{
			LogName = logName;
		}

		public string LogName { get; }

		public void Parse(Line line)
		{
			LastParsedLine = line;
		}

		public Line LastParsedLine { get; set; }
	}
}
