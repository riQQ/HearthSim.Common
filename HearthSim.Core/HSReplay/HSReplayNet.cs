using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.HSReplay.Twitch;

namespace HearthSim.Core.HSReplay
{
	public class HSReplayNet
	{
		internal HSReplayNet(HSReplayNetConfig config)
		{
			Config = config;
			Account = Account.Serializer.Load(config.DataDirectory);
			Api = new ApiWrapper(config, Account);
			OAuth = new OAuthWrapper(config, Account, Api);
			LogUploader = new LogUploader(Api);
			PackUploader = new PackUploader(Api);
			Twitch = new TwitchDataManager(OAuth, config);
		}

		internal HSReplayNetConfig Config { get; }
		public LogUploader LogUploader { get; }
		public Account Account { get; }
		public ApiWrapper Api { get; }
		public OAuthWrapper OAuth { get; }
		public PackUploader PackUploader { get; }
		public TwitchDataManager Twitch { get; }
	}
}
