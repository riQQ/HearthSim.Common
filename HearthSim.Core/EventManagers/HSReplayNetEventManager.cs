using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.HSReplay;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Extensions;

namespace HearthSim.Core.EventManagers
{
	public class HSReplayNetEventManager
	{
		private readonly HSReplayNet _hsReplayNet;
		private readonly Game _game;

		public HSReplayNetEventManager(HSReplayNet hsReplayNet, Game game)
		{
			_hsReplayNet = hsReplayNet;
			_game = game;
			game.PackOpened += Game_OnPackOpened;
			game.GameCreated += Game_GameCreated;
			game.GameEnded += Game_OnGameEnd;
		}

		private void Game_OnPackOpened(PackOpenedEventArgs args)
		{
			if(_hsReplayNet?.Config.UploadPacks ?? false)
				_hsReplayNet.PackUploader.UploadPack(_game.Account, args.Pack);
		}

		private void Game_OnGameEnd(GameEndEventArgs args)
		{
			if(_hsReplayNet == null)
				return;
			_hsReplayNet.Twitch.Stop();
			var matchInfo = args.GameState.MatchInfo;
			var gameType = matchInfo != null
				? Converters.GetBnetGameType((GameType) matchInfo.GameType, (FormatType) matchInfo.FormatType)
				: BnetGameType.BGT_UNKNOWN;
			if(_hsReplayNet.Config.UploadGameTypes.Contains(gameType))
				return;
			var data = UploadMetaDataGenerator.Generate(args.Build, args.GameState, args.Wins, args.Losses);
			_hsReplayNet.LogUploader.Upload(args.GameState.PowerLog.ToArray(), data).Forget();
		}

		private void Game_GameCreated(GameCreatedEventArgs args)
		{
			_hsReplayNet?.Twitch.WatchBoardState(args.Game);
		}
	}
}
