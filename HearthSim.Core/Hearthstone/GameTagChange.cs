using System;
using HearthDb.Enums;

namespace HearthSim.Core.Hearthstone
{
	public struct GameTagChange
	{
		public GameTag GameTag { get; }
		public int PreviousValue { get; }
		public int CurrentValue { get; }

		private GameTagChange(GameTag gameTag, Enum previousValue, Enum currentValue) : this(gameTag,
			Convert.ToInt32(previousValue), Convert.ToInt32(currentValue))
		{
		}

		private GameTagChange(GameTag gameTag, int previousValue, int currentValue)
		{
			GameTag = gameTag;
			PreviousValue = previousValue;
			CurrentValue = currentValue;
		}

		public static implicit operator GameTagChange((GameTag, Enum, Enum) data)
		{
			return new GameTagChange(data.Item1, data.Item2, data.Item3);
		}

		public static implicit operator GameTagChange((GameTag, int, int) data)
		{
			return new GameTagChange(data.Item1, data.Item2, data.Item3);
		}
	}
}
