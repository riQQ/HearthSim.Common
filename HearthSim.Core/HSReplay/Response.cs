using System;

namespace HearthSim.Core.HSReplay
{
	public class Response<T>
	{
		public T Data { get; }
		public Exception Exception { get; }
		public bool Success { get; }

		public Response(T data)
		{
			Data = data;
			Success = true;
		}

		public Response(Exception ex)
		{
			Exception = ex;
			Success = false;
		}
	}
}
