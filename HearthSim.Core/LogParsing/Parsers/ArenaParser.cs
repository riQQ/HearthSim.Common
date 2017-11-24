using System;
using HearthMirror;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class ArenaParser : ILogParser
	{
		public string LogName { get; } = "Arena";

		public void Parse(Line line)
		{
			if(line.Text.Contains("IN_REWARDS"))
			{
				var rewards = Reflection.GetArenaRewards();
				ArenaRunComplete?.Invoke(new ArenaRunCompleteEventArgs(rewards));
			}
		}

		public event Action<ArenaRunCompleteEventArgs> ArenaRunComplete;
	}
}
