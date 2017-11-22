using System;
using HearthDb.Enums;

namespace HearthSim.Core.LogParsing.Parsers.Power
{
	public class GameTagParser
	{
		public static int ParseTag(GameTag tag, string rawValue)
		{
			switch(tag)
			{
				case GameTag.ZONE:
					return (int)ParseEnum<Zone>(rawValue);
				case GameTag.MULLIGAN_STATE:
					return (int)ParseEnum<Mulligan>(rawValue);
				case GameTag.PLAYSTATE:
					return (int)ParseEnum<PlayState>(rawValue);
				case GameTag.CARDTYPE:
					return (int)ParseEnum<CardType>(rawValue);
				case GameTag.CLASS:
					return (int)ParseEnum<CardClass>(rawValue);
				case GameTag.STATE:
					return (int)ParseEnum<State>(rawValue);
				case GameTag.STEP:
					return (int)ParseEnum<Step>(rawValue);
				default:
					int value;
					int.TryParse(rawValue, out value);
					return value;
			}
		}

		public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if(Enum.TryParse(value, out TEnum tEnum))
				return tEnum;
			if(int.TryParse(value, out var i) && Enum.IsDefined(typeof(TEnum), i))
				tEnum = (TEnum)(object)i;
			return tEnum;
		}
	}
}
