using System;
using System.Linq;
using System.Threading.Tasks;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.HSReplay.Twitch.Data;
using HearthSim.Core.Util.Extensions;
using HearthSim.Core.Util.Logging;

namespace HearthSim.Core.HSReplay.Twitch
{
	internal class TwitchDataManager
	{
		private readonly OAuthWrapper _oauth;
		private readonly HSReplayNetConfig _config;
		private BoardStateWatcher _boardStateWatcher;
		private BoardStateWatcher BoardStateWatcher => _boardStateWatcher ?? (_boardStateWatcher = GetBoardStateWatcher());

		public event Action<bool> OnStreamingChecked;

		public TwitchDataManager(OAuthWrapper oauth, HSReplayNetConfig config)
		{
			_oauth = oauth;
			_config = config;
		}

		private BoardStateWatcher GetBoardStateWatcher()
		{
			var boardStateWatcher = new BoardStateWatcher();
			boardStateWatcher.OnNewBoardState += OnNewBoardState;
			return boardStateWatcher;
		}

		public async void WatchBoardState(GameState game)
		{
			if(_running)
				return;
			if(_config.TwitchUpdatesUserId == 0 || (_oauth.TwitchUsers?.Count ?? 0) == 0)
				return;
			var streaming = await TwitchApi.IsStreaming(_config.TwitchUpdatesUserId);
			OnStreamingChecked?.Invoke(streaming);
			if(!streaming)
				return;
			_running = true;
			BoardStateWatcher.Start(game);
		}

		public void Stop()
		{
			if(!_running)
				return;
			BoardStateWatcher.Stop();
			SendUpdate(Payload.GameEnd());
			_running = false;
		}

		private DateTime _lastSent = DateTime.MinValue;
		private int _currentHash;
		private bool _running;

		private async void SendUpdate(Payload payload)
		{
			var hash = payload.GetHashCode();
			_currentHash = hash;
			await Task.Delay(Math.Max(0, 1000 - (int)(DateTime.Now - _lastSent).TotalMilliseconds));
			if(_currentHash == hash)
			{
				_lastSent = DateTime.Now;
				Log.Debug($"Sending payload {hash} (type={payload.Type})");
				_oauth.SendTwitchPayload(_config.TwitchUpdatesUserId, payload).Forget();
			}
			else
				Log.Debug($"Skipped payload {hash} (type={payload.Type})");
		}

		private void OnNewBoardState(BoardState boardState)
		{
			Log.Debug(string.Join(",", boardState.Player.Deck.Cards.Select(x => $"[{x[0]}, {x[1]}, {x[2]}]")));
			SendUpdate(Payload.BoardState(boardState));
		}
	}
}
