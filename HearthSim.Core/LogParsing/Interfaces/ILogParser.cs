using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Interfaces
{
	public interface ILogParser
	{
		string LogName { get; }
		void Parse(Line line);
	}
}
