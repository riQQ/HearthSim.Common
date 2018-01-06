﻿using System;

namespace HearthSim.Core.Util.Extensions
{
	public static class DateTimeExtensions
	{
		public static long ToUnixTime(this DateTime time)
			=> System.Math.Max(0, (long)(time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
	}
}
