using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.HSReplay.Twitch;

namespace HearthSim.Core.HSReplay
{
	public class HSReplayNet
	{
		internal HSReplayNet(HSReplayNetConfig config)
		{
			Account = Account.Serializer.Load(config.DataDirectory);
			Api = new ApiWrapper(config, Account);
			OAuth = new OAuthWrapper(config);
			LogUploader = new LogUploader(Api);
			PackUploader = new PackUploader(Api);
			Twitch = new TwitchDataManager(OAuth, config);
		}

		public LogUploader LogUploader { get; }
		public Account Account { get; }
		public ApiWrapper Api { get; }
		public OAuthWrapper OAuth { get; }
		public PackUploader PackUploader { get; }
		public TwitchDataManager Twitch { get; }
	}
}
