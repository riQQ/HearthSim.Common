using System;

namespace HearthSim.Core.Hearthstone.Events
{
	public class BaseGameEvent<T> : EventArgs
	{
		public T Data { get; }
		public IGameState GameState { get; }

		public BaseGameEvent(T data, IGameState state)
		{
			Data = data;
			GameState = state;
		}
	}
}
