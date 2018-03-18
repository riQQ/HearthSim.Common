using System;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.HSReplay;
using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.Util;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Extensions;
using HearthSim.Util.Logging;
using Account = HearthSim.Core.HSReplay.Data.Account;

namespace HearthSim.Core.EventManagers
{
	public class HSReplayNetEventManager
	{
		private readonly HSReplayNet _hsReplayNet;
		private readonly Game _game;
		private readonly RateLimiter _collectionSyncLimiter;

		public HSReplayNetEventManager(HSReplayNet hsReplayNet, Game game)
		{
			_hsReplayNet = hsReplayNet;
			_game = game;
			_collectionSyncLimiter = new RateLimiter(3, TimeSpan.FromMinutes(2));
			game.PackOpened += Game_OnPackOpened;
			game.GameCreated += Game_GameCreated;
			game.GameEnded += Game_OnGameEnd;
			game.Collection.Changed += Collection_Changed;
		}

		private async void Collection_Changed()
		{
			if(_hsReplayNet?.Config.UploadCollection ?? true)
				return;
			if(!_game.Collection.Cards.Any() || !_game.Account.IsLoaded)
				return;
			var cards = _game.Collection.Cards
				.Select(x => new {x.Normal, x.Golden, x.ToCard()?.Data.DbfId})
				.Where(x => x.DbfId != null)
				.ToDictionary(x => x.DbfId.Value, x => new[] {x.Normal, x.Golden});
			var heroes = _game.Collection.FavoriteHeroes
				.Select(x => new {x.ToCard().Data?.Class, Count = x.Normal + x.Golden})
				.Where(x => x.Class != null)
				.GroupBy(x => x.Class.Value)
				.ToDictionary(x => (int)x.Key, x => x.First().Count);
			var collection = new CollectionData(cards, heroes, _game.Collection.CardBacks,
				_game.Collection.FavoriteCardBack , _game.Collection.Dust);
			var hash = collection.GetHashCode();
			var hi = _game.Account.AccountHi;
			var lo = _game.Account.AccountLo;
			var account = hi + "-" + lo;
			if(_hsReplayNet.Account.CollectionState.TryGetValue(account, out var state) && state.Hash == hash)
			{
				Log.Debug("Collection already up to date.");
				state.Date = DateTime.Now;
				_hsReplayNet.Account.Save();
				_hsReplayNet.Events.OnCollectionAlreadyUpToDate();
			}
			await _collectionSyncLimiter.Run(async () =>
			{
				if(!_hsReplayNet.OAuth.AccountData?.BlizzardAccounts?.Any(x => x.AccountHi == hi && x.AccountLo == lo) ?? true)
				{
					var claimed = await _hsReplayNet.OAuth.ClaimBlizzardAccount(hi, lo, _game.Account.BattleTag);
					if(claimed)
						_hsReplayNet.OAuth.UpdateAccountData().Forget();
					else
					{
						_hsReplayNet.Events.OnCollectionUploadError($"Could not register your Blizzard account ({account}). Please try again later.");
						return;
					}
				}
				if(await _hsReplayNet.OAuth.UpdateCollection(collection, _game.Account))
				{
					_hsReplayNet.Account.CollectionState[account] = new Account.SyncState(hash);
					_hsReplayNet.Account.Save();
					Log.Debug("Collection synced");
					_hsReplayNet.Events.OnCollectionUploaded();
				}
				else
					_hsReplayNet.Events.OnCollectionUploadError("Could not update your collection. Please try again later.");
			}, () =>
			{
				Log.Debug("Waiting for rate limit...");
				_hsReplayNet.Events.OnCollectionUploadThrottled();
			});
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
			if(!_hsReplayNet.Config.UploadGameTypes.Contains(gameType))
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
