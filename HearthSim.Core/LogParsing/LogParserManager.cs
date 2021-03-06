﻿using System.Collections.Generic;
using HearthSim.Core.LogParsing.Interfaces;
using HearthSim.Core.LogReading.Data;

namespace HearthSim.Core.LogParsing
{
	public class LogParserManager
	{
		private readonly Dictionary<string, List<ILogParser>> _parsers;

		public LogParserManager()
		{
			_parsers = new Dictionary<string, List<ILogParser>>();
		}

		private List<ILogParser> GetParsers(string logName)
		{
			if(_parsers.TryGetValue(logName, out var parsers))
				return parsers;
			return _parsers[logName] = new List<ILogParser>();
		}

		public void RegisterParser(ILogParser parser)
		{
			GetParsers(parser.LogName).Add(parser);
		}

		public void UnregisterParser(ILogParser parser)
		{
			GetParsers(parser.LogName).Remove(parser);
		}

		public void Parse(IEnumerable<Line> lines)
		{
			foreach(var line in lines)
			{
				foreach(var parser in GetParsers(line.LogName))
					parser.Parse(line);
			}
		}
	}
}
