using System;

namespace HearthSim.Util.Logging
{
	internal class LogWriterCreationException : Exception
	{
		public LogWriterCreationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
