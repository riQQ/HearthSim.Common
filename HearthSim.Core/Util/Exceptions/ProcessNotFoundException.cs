using System;

namespace HearthSim.Core.Util.Exceptions
{
	public class ProcessNotFoundException : Exception
	{
		public ProcessNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
