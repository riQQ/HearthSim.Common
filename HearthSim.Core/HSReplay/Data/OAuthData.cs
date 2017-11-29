using System;
using System.Collections.Generic;
using HearthSim.Core.Util;
using HSReplay.OAuth.Data;
using HSReplay.Responses;

namespace HearthSim.Core.HSReplay.Data
{
	internal class OAuthData : IJsonSerializable
	{
		public static JsonSerializer<OAuthData> Serializer { get; } = new JsonSerializer<OAuthData>("hsreplay_oauth", true);

		public string Code { get; set; }
		public string RedirectUrl { get; set; }
		public TokenData TokenData { get; set; }
		public DateTime TokenDataCreatedAt { get; set; }
		public List<TwitchAccount> TwitchUsers { get; set; }
		public User Account { get; set; }

		public string DataDirectory { get; set; }
		public void Save() => Serializer.Save(this);
	}
}
