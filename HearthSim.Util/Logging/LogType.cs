using System;

namespace HearthSim.Util.Logging
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
