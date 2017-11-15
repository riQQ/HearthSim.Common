using System;
using System.Collections.Generic;
using System.Linq;

namespace HearthSim.Core.LogReading.Data
{
	public class LogWatcherData
	{
		public LogWatcherData(string name, string[] entryPoints, params Func<string, bool>[] filters)
		{
			Name = name;
			EntryPoints = entryPoints;
			Filters = filters.ToList();
		}

		public bool HasFilters => Filters.Any();
		public string Name { get; }
		public string[] EntryPoints { get; }
		public IReadOnlyList<Func<string, bool>> Filters { get; }
	}
}
