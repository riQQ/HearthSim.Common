using System;

namespace HearthSim.Core.Util.EventArgs
{
	public class LogConfigErrorEventArgs
	{
		public LogConfigErrorEventArgs(Exception exception)
		{
			Exception = exception;
		}

		public Exception Exception { get; }
	}
}
