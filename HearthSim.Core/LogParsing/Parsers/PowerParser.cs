using System;
using HearthDb.Enums;
using HearthSim.Core.LogParsing.EventArgs;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing.Parsers
{
	public class PowerParser : ILogParser
	{
		public string LogName { get; } = "Power";

		public event Action<PowerActionEventArgs> PowerAction;
		public event Action<GameStateLogEventArgs> GameStateLog;

		public void Parse(Line line)
		{
			if(line.Text.StartsWith("GameState."))
				GameStateLog?.Invoke(new GameStateLogEventArgs(line));
			else
			{
				var action = HandlePowerTaskList(line);
				if (action != null)
					PowerAction?.Invoke(new PowerActionEventArgs(action));
			}
		}

		private PowerAction HandlePowerTaskList(Line line)
		{
			if(line.Text.StartsWith("CREATE_GAME"))
				return new PowerAction(PowerType.CREATE_GAME);
			return null;
		}
	}
}
