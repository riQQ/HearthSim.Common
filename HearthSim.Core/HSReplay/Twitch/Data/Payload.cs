using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HearthSim.Core.HSReplay.Twitch.Data
{
	public class Payload
	{
		[JsonProperty("type")]
		[JsonConverter(typeof(StringEnumConverter))]
		public DataType Type { get; set; }

		[JsonProperty("data")]
		public object Data { get; set; }

		[JsonProperty("version")]
		public int Version => 3;

		public static Payload GameEnd()
		{
			return new Payload
			{
				Type = DataType.GameEnd,
				Data = new Dictionary<object, object>()
			};
		}

		public static Payload BoardState(BoardState boardState)
		{
			return new Payload
			{
				Type = DataType.BoardState,
				Data = boardState,
			};
		}

		public static Payload GameStart(GameStart gameStart)
		{
			return new Payload
			{
				Type = DataType.GameStart,
				Data = gameStart
			};
		}
	}
}
