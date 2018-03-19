using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthSim.Core.HSReplay.Data;
using HearthSim.Core.HSReplay.Twitch.Data;
using HearthSim.Core.Util.Exceptions;
using HearthSim.Util.Logging;
using HSReplay.OAuth;
using HSReplay.OAuth.Data;
using HSReplay.Responses;
using Newtonsoft.Json.Linq;
using static HearthSim.Core.Util.JsonHelper;
using Account = HearthSim.Core.Hearthstone.Account;

namespace HearthSim.Core.HSReplay
{
	public sealed class OAuthWrapper
	{
		private readonly HSReplayNetConfig _config;
		private readonly Data.Account _account;
		private readonly ApiWrapper _api;
		private readonly UploadTokenHistory _uploadTokenHistory;
		private Lazy<OAuthClient> _client;
		private OAuthData _data;

		private readonly int[] _ports = { 17781, 17782, 17783, 17784, 17785, 17786, 17787, 17788, 17789 };
		private const string TwitchExtensionId = "apwln3g3ia45kk690tzabfp525h9e1";
		private const string SuccessUrl = "https://hsdecktracker.net/hsreplaynet/oauth_success/";
		private const string ErrorUrl = "https://hsdecktracker.net/hsreplaynet/oauth_error/";

		public event Action<string> AuthenticationError;
		public event Action CollectionUpdated;
		public event Action Authenticated;
		public event Action LoggedOut;
		public event Action TwitchUsersUpdated;
		public event Action AccountDataUpdated;
		public event Action UploadTokenClaimed;
		public event Action<bool> Authenticating;

		private readonly Scope[] _requiredScopes = { Scope.FullAccess };

		internal OAuthWrapper(HSReplayNetConfig config, Data.Account account, ApiWrapper api)
		{
			_config = config;
			_account = account;
			_api = api;
			_uploadTokenHistory = new UploadTokenHistory(config.DataDirectory);
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

		public bool IsFullyAuthenticated => IsAuthenticatedFor(_requiredScopes);

		public bool IsAuthenticatedFor(params Scope[] scopes)
		{
			if(string.IsNullOrEmpty(_data.TokenData?.Scope))
				return false;
			var currentScopes = _data.TokenData.Scope.Split(' ');
			if(currentScopes.Contains(Scope.FullAccess.Name))
				return true;
			return scopes.All(s => currentScopes.Contains(s.Name));
		}

		public bool IsAuthenticatedForAnything()
			=> !string.IsNullOrEmpty(_data.TokenData?.Scope);


		public async Task Authenticate(params Scope[] scopes)
		{
			Log.Debug("Authenticating with HSReplay.net...");
			Authenticating?.Invoke(true);
			try
			{
				var data = await GetAuthData();
				if(data == null)
				{
					Log.Error("Authentication failed, received no data");
					return;
				}
				_data.Code = data.Code;
				_data.RedirectUrl = data.RedirectUrl;
				_data.TokenData = null;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return;
			}
			Log.Debug("Authentication complete");
			try
			{
				await UpdateToken();
				Log.Debug("Claiming upload token if necessary");
				if(_account.TokenStatus == TokenStatus.Unknown)
					await _api.UpdateTokenStatus();
				if(_account.TokenStatus == TokenStatus.Unclaimed)
					await ClaimUploadToken(_account.UploadToken);
				Log.Debug("Updating account data");
				if(!await UpdateAccountData())
				{
					AuthenticationError?.Invoke("Could not load HSReplay.net account status. "
												+ "Please try again later.");
				}
			}
			catch(Exception e)
			{
				AuthenticationError?.Invoke("Could not authenticate with HSReplay.net.");
				Log.Error(e);
			}
			finally
			{
				Authenticating?.Invoke(false);
				Authenticated?.Invoke();
			}

		}

		private async Task<AuthData> GetAuthData()
		{
			var url = _client.Value.GetAuthenticationUrl(_requiredScopes, _ports);
			if(string.IsNullOrEmpty(url))
				throw new Exception("Authentication failed, could not create callback listener");
			var callbackTask = _client.Value.ReceiveAuthenticationCallback(SuccessUrl, ErrorUrl);
			try
			{
				Process.Start(url);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				AuthenticationError?.Invoke("Could not open your browser. "
					+ "Please open the following url in your browser to continue:\n\n" + url);
			}
			Log.Debug("Waiting for callback...");
			return await callbackTask;
		}
		
		public async Task Logout()
		{
			OAuthData.Serializer.Delete(_data);
			_data.Account = null;
			_data.Code = null;
			_data.RedirectUrl = null;
			_data.TokenData = null;
			_data.TokenDataCreatedAt = DateTime.MinValue;
			_data.TwitchUsers = null;
			OAuthData.Serializer.Save(_data);
			_account.Reset();
			_uploadTokenHistory.Write("Deleting token");
			Load();
			await _api.UpdateTokenStatus();
			LoggedOut?.Invoke();
		}

		/// <summary>
		/// Ensure OAuth token is up-to-date.
		/// Will refresh token when expired or forced
		/// </summary>
		/// <param name="force">Force update regardless expiration date</param>
		/// <exception cref="TokenUpdateFailedException"></exception>
		public async Task UpdateToken(bool force = false)
		{
			if(!force && _data.TokenData != null && (DateTime.Now - _data.TokenDataCreatedAt).TotalSeconds < _data.TokenData.ExpiresIn)
				return;
			if(string.IsNullOrEmpty(_data.Code) || string.IsNullOrEmpty(_data.RedirectUrl))
				throw new TokenUpdateFailedException("Could not update token, we don't have a code or redirect url.");
			if(!string.IsNullOrEmpty(_data.TokenData?.RefreshToken))
			{
				Log.Debug("Refreshing token data...");
				try
				{
					var tokenData = await _client.Value.RefreshToken();
					if(tokenData != null)
					{
						SaveTokenData(tokenData);
						return;
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
				Log.Debug("Fetching new token...");
				var tokenData = await _client.Value.GetToken(_data.Code, _data.RedirectUrl);
				if(tokenData == null)
					throw new TokenUpdateFailedException("We did not receive any token data.");
				SaveTokenData(tokenData);
			}
			catch(Exception e)
			{
				throw new TokenUpdateFailedException("Fetching a new token failed.", e);
			}
		}

		public async Task<bool> UpdateTwitchUsers()
		{
			Log.Debug("Fetching twitch accounts...");
			try
			{
				await UpdateToken();
				var twitchAccounts = await _client.Value.GetTwitchAccounts();
				_data.TwitchUsers = twitchAccounts;
				_data.Save();
				Log.Debug($"Saved {twitchAccounts.Count} account(s): {string.Join(", ", twitchAccounts.Select(x => x.Username))}");
				TwitchUsersUpdated?.Invoke();
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
			Log.Debug("Updating account data...");
			try
			{
				await UpdateToken();
				var account = await _client.Value.GetHSReplayNetAccount();
				_data.Account = account;
				_data.Save();
				Log.Debug($"Found account: {account?.Username ?? "None"}");
				AccountDataUpdated?.Invoke();
				if(account == null)
					return false;
				_account.Update(account.Id, account.Username);
				return true;
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
			Log.Debug("Saved token data");
		}

		public async Task SendTwitchPayload(int twitchUserId, Payload payload)
		{
			try
			{
				await UpdateToken();
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
			Log.Debug("Fetching archetype matchups");
			try
			{
				await UpdateToken();
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
			Log.Debug("Fetching archetype mulligan");
			try
			{
				await UpdateToken();
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
		
		public async Task<bool> UpdateCollection(CollectionData collection, Account account)
		{
			try
			{
				await UpdateToken();
				var response = await _client.Value.UploadCollection(collection, account.AccountHi, account.AccountLo);
				Log.Debug(response);
				CollectionUpdated?.Invoke();
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		internal async Task<bool> ClaimUploadToken(string token)
		{
			_uploadTokenHistory.Write("Trying to claim " + token);
			try
			{
				await UpdateToken();
				var response = await _client.Value.ClaimUploadToken(token);
				_uploadTokenHistory.Write($"Claimed {token}: {response}");
				Log.Debug(response);
				_account.TokenStatus = TokenStatus.Claimed;
				_account.Save();
				UploadTokenClaimed?.Invoke();
				return true;
			}
			catch(Exception e)
			{
				_uploadTokenHistory.Write($"Error claming {token}\n" + e);
				Log.Error(e);
				return false;
			}
		}

		internal async Task<bool> ClaimBlizzardAccount(ulong accountHi, ulong accountLo, string battleTag)
		{
			var account = $"hi={accountHi}, lo={accountLo}, battleTag={battleTag}";
			try
			{
				await UpdateToken();
				var response = await _client.Value.ClaimBlizzardAccount(accountHi, accountLo, battleTag);
				Log.Debug($"Claimed {account}: {response}");
				return true;
			}
			catch(Exception e)
			{
				Log.Error($"Error claming {account}\n" + e);
				return false;
			}
		}
	}
}
