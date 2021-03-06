﻿using System.Threading.Tasks;

namespace HearthSim.Util.Analytics
{
	public abstract class InfluxClient
	{
		public async Task Send(InfluxPointBuilder builder)
		{
			await Send(builder.Build());
		}

		public abstract Task Send(InfluxPoint point);
	}
}
