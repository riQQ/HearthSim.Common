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
			game.Collection.Changed += UploadCollection;
			hsReplayNet.OAuth.Authenticated += UploadCollection;
		}

		private string Account => _game.Account.AccountHi + "-" + _game.Account.AccountLo;

		private CollectionData GetCollection()
		{
			if(!_hsReplayNet?.Config.UploadCollection ?? true)
				return null;
			if(!_game.Collection.Cards.Any() || !_game.Account.IsLoaded)
				return null;
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
			return collection;
		}

		private async void UploadCollection()
		{
			var collection = GetCollection();
			if(collection == null)
			{
				_hsReplayNet.Events.OnCollectionUploadError(CollectionUploadError.CollectionNotFound);
				return;
			}
			if(_hsReplayNet.Account.CollectionState.TryGetValue(Account, out var state)
				&& state.Hash == collection.GetHashCode())
			{
				Log.Debug("Collection already up to date.");
				state.Date = DateTime.Now;
				_hsReplayNet.Account.Save();
				_hsReplayNet.Events.OnCollectionAlreadyUpToDate();
				return;
			}
			await _collectionSyncLimiter.Run(async () =>
			{
				var hi = _game.Account.AccountHi;
				var lo = _game.Account.AccountLo;
				var battleTag = _game.Account.BattleTag;
				if(!_hsReplayNet.OAuth.AccountData?.BlizzardAccounts?.Any(x => x.AccountHi == hi && x.AccountLo == lo) ?? true)
				{
					var error = await _hsReplayNet.OAuth.ClaimBlizzardAccount(hi, lo, battleTag);
					if(error != ClaimError.None)
					{
						_hsReplayNet.Events.OnBlizzardAccountClaimError(error, hi, lo, battleTag);
						return;
					}
					_hsReplayNet.Events.OnBlizzardAccountClaimed(hi, lo, battleTag);
					_hsReplayNet.OAuth.UpdateAccountData().Forget();
				}

				if(await _hsReplayNet.OAuth.UpdateCollection(collection, _game.Account))
				{
					_hsReplayNet.Account.CollectionState[Account] = new Account.SyncState(collection.GetHashCode());
					_hsReplayNet.Account.Save();
					Log.Debug("Collection synced");
					_hsReplayNet.Events.OnCollectionUploaded();
				}
				else
					_hsReplayNet.Events.OnCollectionUploadError(CollectionUploadError.Unknown);
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
