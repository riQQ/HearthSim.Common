﻿using System;
using Newtonsoft.Json;

namespace HearthSim.Core.HSReplay.Data
{
	public class HsReplayData
	{
		private readonly int _maxAgeHours;

		public HsReplayData(int maxAgeHours = 24)
		{
			_maxAgeHours = maxAgeHours;
		}

		public DateTime ServerTimeStamp { get; set; }

		public DateTime ClientTimeStamp { get; set; }

		[JsonIgnore]
		public TimeSpan Age => DateTime.Now - ClientTimeStamp;

		[JsonIgnore]
		public bool IsStale => Age.TotalHours > _maxAgeHours;
	}
}
