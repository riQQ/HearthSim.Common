using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthDb.Deckstrings;
using HearthSim.Core.HSReplay.Data;
using HearthSim.Util.Logging;
using HSReplay;
using HSReplay.Responses;
using Newtonsoft.Json.Linq;
using static HearthSim.Core.Util.JsonHelper;

namespace HearthSim.Core.HSReplay
{
	public class ApiWrapper
	{
		private readonly Account _account;
		private readonly HsReplayClient _client;
		private bool _requestedNewToken;

		internal ApiWrapper(HSReplayNetConfig config, Account account)
		{
			_account = account;
			_client = new HsReplayClient(config.ApiKey, config.UserAgent);
		}

		private async Task<string> GetUploadToken()
		{
			if(!string.IsNullOrEmpty(_account.UploadToken))
				return _account.UploadToken;
			string token;
			try
			{
				Log.Debug("Requesting new upload token...");
				token = await _client.CreateUploadToken();
				if(string.IsNullOrEmpty(token))
					throw new Exception("Reponse contained no upload-token.");
			}
			catch(Exception e)
			{
				Log.Error(e);
				throw new Exception("Webrequest to obtain upload-token failed.", e);
			}
			_account.UploadToken = token;
			_account.Save();
			Log.Debug("Received new upload-token.");
			return token;
		}

		public async Task<Response<TokenStatus>> UpdateTokenStatus()
		{
			Log.Debug("Checking account status...");
			try
			{
				var token = await GetUploadToken();
				var accountStatus = await _client.GetAccountStatus(token);
				_account.TokenStatus = accountStatus?.User != null ? TokenStatus.Claimed : TokenStatus.Unclaimed;
				_account.Save();
				Log.Info($"Token is {_account.TokenStatus}");
				return new Response<TokenStatus>(_account.TokenStatus);
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				var response = ex.Response as HttpWebResponse;
				if(response?.StatusCode == HttpStatusCode.NotFound && !_requestedNewToken)
				{
					_requestedNewToken = true;
					_account.UploadToken = string.Empty;
					return await UpdateTokenStatus();
				}
				return new Response<TokenStatus>(ex);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return new Response<TokenStatus>(ex);
			}
		}

		internal async Task<LogUploadRequest> CreateUploadRequest(UploadMetaData metaData) 
			=> await _client.CreateUploadRequest(metaData, await GetUploadToken());


		internal async Task UploadLog(LogUploadRequest uploadRequest, string[] logLines) 
			=> await _client.UploadLog(uploadRequest, logLines);

		internal async Task UploadPack(PackData data) => await _client.UploadPack(data, await GetUploadToken());

		public async Task<Response<DecksData>> GetAvailableDecks()
		{
			Log.Debug("Fetching available decks...");
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetAvailableDecks(token);
				if(data == null)
					return null;
				return new Response<DecksData>(
					new DecksData
					{
						ClientTimeStamp = DateTime.Now,
						ServerTimeStamp = data.ServerTimeStamp,
						Decks = data.Data.Properties().Select(p => p.Name).ToArray()
					}
				);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<DecksData>(e);
			}
		}

		public async Task<Response<DeckWinrateData>> GetDeckWinrates(string deckId, bool wild)
		{
			Log.Debug("Fetching winrates for deck " + deckId);
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetDeckWinrates(deckId, wild, token);
				if(data == null)
					return null;

				var winrates = GetChildren(data.Data["data"]);
				var dict = winrates.ToDictionary(
					x => x.Name,
					x => x.Value[0]["winrate"].Value<double>()
				);

				var totalWinrate = data.Data.SelectToken("metadata.total_winrate").Value<double>();

				return new Response<DeckWinrateData>(
					new DeckWinrateData
					{
						ClientTimeStamp = DateTime.Now,
						ServerTimeStamp = data.ServerTimeStamp,
						TotalWinrate = totalWinrate,
						ClassWinrates = dict
					}
				);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<DeckWinrateData>(e);
			}
		}

		public async Task<Response<List<Archetype>>> GetArchetypes()
		{
			Log.Debug("Fetching archetypes");
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetArchetypes(token);
				return data == null ? null : new Response<List<Archetype>>(data);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<List<Archetype>>(e);
			}
		}

		public async Task<Response<ArchetypeMatchupsData>> GetArchetypeMatchups()
		{
			Log.Debug("Fetching archetype matchups");
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetArchetypeMatchups(token);
				var matchups = GetChildren(data.Data["data"]);
				var dict = matchups.ToDictionary(
					x => x.Name,
					x => GetChildren(x.Value).ToDictionary(
						y => y.Name,
						y => y.Value["win_rate"].Value<double>()
					)
				);
				return new Response<ArchetypeMatchupsData>(
					new ArchetypeMatchupsData
					{
						ArchetypeMatchups = dict,
						ClientTimeStamp = DateTime.Now,
						ServerTimeStamp = data.ServerTimeStamp
					}
				);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<ArchetypeMatchupsData>(e);
			}
		}

		public async Task<Response<ArchetypeMulliganData>> GetArchetypeMulligan(int archetypeId)
		{
			Log.Debug("Fetching archetype mulligan");
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetArchetypeMulligan(token, archetypeId);
				var archetypes = data.Data.SelectToken("data.ALL").Children();
				return new Response<ArchetypeMulliganData>(
					new ArchetypeMulliganData
					{
						MulliganData = archetypes.Select(x => x.ToObject<MulliganData>()).ToList(),
						ClientTimeStamp = DateTime.Now,
						ServerTimeStamp = data.ServerTimeStamp
					}
				);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<ArchetypeMulliganData>(e);
			}
		}

		public async Task<Response<DeckData>> GetDeck(Deck deck)
		{
			Log.Debug("Fetching deck data");
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetDeck(token, (int) deck.Format, deck.HeroDbfId,
					deck.CardDbfIds.SelectMany(x => Enumerable.Repeat(x.Key, x.Value)));
				return new Response<DeckData>(data);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<DeckData>(e);
			}
		}
	}
}
