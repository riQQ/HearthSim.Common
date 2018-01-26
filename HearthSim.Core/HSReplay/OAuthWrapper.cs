using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.HSReplay.Twitch.Data;
using HearthSim.Util.Logging;
using HSReplay.OAuth;
using HSReplay.OAuth.Data;
using HSReplay.Responses;
using Newtonsoft.Json.Linq;
using static HearthSim.Core.Util.JsonHelper;

namespace HearthSim.Core.HSReplay
{
	public sealed class OAuthWrapper
	{
		private readonly HSReplayNetConfig _config;
		private Lazy<OAuthClient> _client;
		private OAuthData _data;

		private readonly int[] _ports = { 17781, 17782, 17783, 17784, 17785, 17786, 17787, 17788, 17789 };
		private const string TwitchExtensionId = "apwln3g3ia45kk690tzabfp525h9e1";
		private const string SuccessUrl = "https://hsdecktracker.net/hsreplaynet/oauth_success/";
		private const string ErrorUrl = "https://hsdecktracker.net/hsreplaynet/oauth_error/";

		public event Action<string> AuthenticationError;

		internal OAuthWrapper(HSReplayNetConfig config)
		{
			_config = config;
			Load();
		}

		private void Load()
		{
			_data = OAuthData.Serializer.Load(_config.DataDirectory);
			_client = new Lazy<OAuthClient>(LoadClient);
		}

		private OAuthClient LoadClient()
		{
			return new OAuthClient(_config.OAuthKey, _config.UserAgent, _data.TokenData);
		}

		public async Task<bool> Authenticate(params Scope[] scopes)
		{
			Log.Info("Authenticating with HSReplay.net...");
			string url;
			try
			{
				url = _client.Value.GetAuthenticationUrl(scopes, _ports);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
			if(string.IsNullOrEmpty(url))
			{
				Log.Error("Authentication failed, could not create callback listener");
				return false;
			}
			var callbackTask = _client.Value.ReceiveAuthenticationCallback(SuccessUrl, ErrorUrl);
			try
			{
				Process.Start(url);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				AuthenticationError?.Invoke($"Could not open browser to complete authentication. Please go to '{url}' to continue authentication.");
			}
			Log.Info("Waiting for callback...");
			var data = await callbackTask;
			if(data == null)
			{
				Log.Error("Authentication failed, received no data");
				return false;
			}
			_data.Code = data.Code;
			_data.RedirectUrl = data.RedirectUrl;
			Log.Info("Authentication complete");
			_data.Save();
			return await UpdateToken();
		}

		public void DeleteToken()
		{
			OAuthData.Serializer.Delete(_data);
			Load();
		}

		public async Task<bool> UpdateToken(bool force = false)
		{
			if(!force && _data.TokenData != null && (DateTime.Now - _data.TokenDataCreatedAt).TotalSeconds < _data.TokenData.ExpiresIn)
				return true;
			if(string.IsNullOrEmpty(_data.Code) || string.IsNullOrEmpty(_data.RedirectUrl))
			{
				Log.Error("Could not update token, we don't have a code or redirect url.");
				return false;
			}
			if(!string.IsNullOrEmpty(_data.TokenData?.RefreshToken))
			{
				Log.Info("Refreshing token data...");
				try
				{
					var tokenData = await _client.Value.RefreshToken();
					if(tokenData != null)
					{
						SaveTokenData(tokenData);
						return true;
					}
				}
				catch(WebException e)
				{
					Log.Error(e);
					if(e.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.Unauthorized)
						SaveTokenData(null);
				}
				catch(Exception e)
				{
					Log.Error(e);
				}
			}
			try
			{
				Log.Info("Fetching new token...");
				var tokenData = await _client.Value.GetToken(_data.Code, _data.RedirectUrl);
				if(tokenData == null)
				{
					Log.Error("We did not receive any token data.");
					return false;
				}
				SaveTokenData(tokenData);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public async Task<bool> UpdateTwitchUsers()
		{
			Log.Info("Fetching twitch accounts...");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return false;
				}
				var twitchAccounts = await _client.Value.GetTwitchAccounts();
				_data.TwitchUsers = twitchAccounts;
				_data.Save();
				Log.Info($"Saved {twitchAccounts.Count} account(s): {string.Join(", ", twitchAccounts.Select(x => x.Username))}");
				return twitchAccounts.Count != 0;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public async Task<bool> UpdateAccountData()
		{
			Log.Info("Updating account data...");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return false;
				}
				var account = await _client.Value.GetHSReplayNetAccount();
				_data.Account = account;
				_data.Save();
				Log.Info($"Found account: {account?.Username ?? "None"}");
				return account != null;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public bool IsAuthenticated => !string.IsNullOrEmpty(_data.Code);

		public List<TwitchAccount> TwitchUsers => _data.TwitchUsers;

		public User AccountData => _data.Account;

		private void SaveTokenData(TokenData data)
		{
			_data.TokenData = data;
			_data.TokenDataCreatedAt = DateTime.Now;
			_data.Save();
			Log.Info("Saved token data");
		}

		public async Task SendTwitchPayload(int twitchUserId, Payload payload)
		{
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return;
				}
				var response = await _client.Value.SendTwitchUpdate(twitchUserId, TwitchExtensionId, payload);
				Log.Debug(response);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public async Task<Response<ArchetypeMatchupsData>> GetArchetypeMatchups(string rankRange)
		{
			Log.Info("Fetching archetype matchups");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return new Response<ArchetypeMatchupsData>(new Exception("Could not update token"));
				}
				var data = await _client.Value.GetArchetypeMatchups(rankRange);
				var matchups = GetChildren(data.Data["data"]);
				var dict = matchups.ToDictionary(
					x => x.Name,
					x => GetChildren(x.Value)
						.Where(y => y.Value["total_games"].Value<int>() >= 30)
						.ToDictionary(
							y => y.Name,
							y => y.Value["win_rate"].Value<double>())
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

		public async Task<Response<ArchetypeMulliganData>> GetArchetypeMulligan(int archetypeId, string rankRange)
		{
			Log.Info("Fetching archetype mulligan");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return new Response<ArchetypeMulliganData>(new Exception("Could not update token"));
				}
				var data = await _client.Value.GetArchetypeMulligan(archetypeId, rankRange);
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
	}
}
