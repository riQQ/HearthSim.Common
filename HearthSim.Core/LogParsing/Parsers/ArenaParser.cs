using System;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class ArenaParser : ILogParser
	{
		public string LogName { get; } = "Arena";

		public void Parse(Line line)
		{
			if(line.Text.Contains("IN_REWARDS"))
				ArenaRunComplete?.Invoke();
		}

		public event Action ArenaRunComplete;
	}
}
