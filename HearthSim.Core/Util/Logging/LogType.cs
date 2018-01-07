using System;

namespace HearthSim.Core.Util.Logging
{
	[Flags]
	public enum LogType
	{
		Debug,
		Info,
		Warning,
		Error,
		Fatal
	}
}
