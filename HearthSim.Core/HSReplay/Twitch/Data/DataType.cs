﻿using System.Runtime.Serialization;

namespace HearthSim.Core.HSReplay.Twitch.Data
{
	public enum DataType
	{
		[EnumMember(Value = "board_state")]
		BoardState,

		[EnumMember(Value = "game_end")]
		GameEnd,

		[EnumMember(Value = "game_start")]
		GameStart
	}
}
