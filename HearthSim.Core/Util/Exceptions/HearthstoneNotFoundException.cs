using System;

namespace HearthSim.Core.Util.Exceptions
{
	public class HearthstoneNotFoundException : Exception
	{
		public HearthstoneNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
