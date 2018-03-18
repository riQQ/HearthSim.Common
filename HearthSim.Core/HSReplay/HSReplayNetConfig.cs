using System.Collections.Generic;
using HearthDb.Enums;

namespace HearthSim.Core.HSReplay
{
	public class HSReplayNetConfig
	{
		public HSReplayNetConfig(string dataDirectory, string apiKey, string oAuthKey, string userAgent, IEnumerable<BnetGameType> uploadGameTypes, bool uploadPacks, int twitchUpdatesUserId)
		{
			DataDirectory = dataDirectory;
			ApiKey = apiKey;
			OAuthKey = oAuthKey;
			UserAgent = userAgent;
			UploadGameTypes = uploadGameTypes;
			UploadPacks = uploadPacks;
			TwitchUpdatesUserId = twitchUpdatesUserId;
		}

		public string OAuthKey { get; }
		public string DataDirectory { get; }
		public string ApiKey { get; }
		public string UserAgent { get; }
		public IEnumerable<BnetGameType> UploadGameTypes { get; set; }
		public bool UploadPacks { get; set; }
		public bool UploadCollection { get; set; } = true;
		public int TwitchUpdatesUserId { get; set; }
	}
}
