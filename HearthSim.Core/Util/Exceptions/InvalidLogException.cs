using System;

namespace HearthSim.Core.Util.Exceptions
{
	public class InvalidLogException : Exception
	{
		public string Reason { get; }

		public InvalidLogException(string reason)
		{
			Reason = reason;
		}
	}
}
