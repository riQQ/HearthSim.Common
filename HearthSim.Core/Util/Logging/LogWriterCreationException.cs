using System;

namespace HearthSim.Core.Util.Logging
{
	internal class LogWriterCreationException : Exception
	{
		public LogWriterCreationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
