using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.Util.Logging;
using HSReplay;
using HSReplay.Responses;
using Newtonsoft.Json.Linq;

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
				Log.Info("Requesting new upload token...");
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
			Log.Info("Received new upload-token.");
			return token;
		}

		public async Task<Response<string>> GetClaimAccountUrl()
		{
			try
			{
				var token = await GetUploadToken();
				Log.Info("Getting claim url...");
				var url =  await _client.GetClaimAccountUrl(token);
				return new Response<string>(url);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new Response<string>(e);
			}
		}

		public async Task<Response<Account>> UpdateAccountStatus()
		{
			Log.Info("Checking account status...");
			try
			{
				var token = await GetUploadToken();
				var accountStatus = await _client.GetAccountStatus(token);
				_account.Id = accountStatus?.User?.Id ?? 0;
				_account.Username = accountStatus?.User?.Username;
				_account.Status = accountStatus?.User != null ? AccountStatus.Registered : AccountStatus.Anonymous;
				_account.LastUpdated = DateTime.Now;
				_account.Save();
				Log.Info($"Id={_account.Id}, Username={_account.Username}, Status={_account.Status}");
				return new Response<Account>(_account);
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				var response = ex.Response as HttpWebResponse;
				if(response?.StatusCode == HttpStatusCode.NotFound && !_requestedNewToken)
				{
					_requestedNewToken = true;
					_account.UploadToken = string.Empty;
					return await UpdateAccountStatus();
				}
				return new Response<Account>(ex);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return new Response<Account>(ex);
			}
		}

		internal async Task<LogUploadRequest> CreateUploadRequest(UploadMetaData metaData) 
			=> await _client.CreateUploadRequest(metaData, await GetUploadToken());


		internal async Task UploadLog(LogUploadRequest uploadRequest, string[] logLines) 
			=> await _client.UploadLog(uploadRequest, logLines);

		internal async Task UploadPack(PackData data) => await _client.UploadPack(data, await GetUploadToken());

		public async Task<Response<DecksData>> GetAvailableDecks()
		{
			Log.Info("Fetching available decks...");
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
			Log.Info("Fetching winrates for deck " + deckId);
			try
			{
				var token = await GetUploadToken();
				var data = await _client.GetDeckWinrates(deckId, wild, token);
				if(data == null)
					return null;

				var winrates = data.Data["data"].Children().OfType<JProperty>().Where(x => x.Values().Any());
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

		public class Response<T>
		{
			public T Data { get; }
			public Exception Exception { get; }
			public bool Success { get; }

			public Response(T data)
			{
				Data = data;
				Success = true;
			}

			public Response(Exception ex)
			{
				Exception = ex;
				Success = false;
			}
		}
	}
}
