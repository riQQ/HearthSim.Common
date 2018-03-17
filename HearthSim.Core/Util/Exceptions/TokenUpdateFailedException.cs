using System;

namespace HearthSim.Core.Util.Exceptions
{
	public class TokenUpdateFailedException : Exception
	{
		public TokenUpdateFailedException(string message) : base(message)
		{
		}

		public TokenUpdateFailedException(string message, Exception innerException) 
			: base(message, innerException)
		{
		}
	}
}
