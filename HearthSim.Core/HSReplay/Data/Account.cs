using System;
using System.Collections.Generic;
using HearthSim.Core.Util;
using HearthSim.Core.Util.EventArgs;
using Newtonsoft.Json;

namespace HearthSim.Core.HSReplay.Data
{
	public class Account : IJsonSerializable
	{
		private TokenStatus _tokenStatus;
		public static JsonSerializer<Account> Serializer { get; } = new JsonSerializer<Account>("hsreplay.cache", false);

		[JsonIgnore]
		public AccountStatus Status => Id == 0 ? AccountStatus.Anonymous : AccountStatus.Registered;

		public string UploadToken { get; set; }
		public string Username { get; set; }
		public int Id { get; set; }
		public DateTime LastUpdated { get; set; }
		public Dictionary<string, SyncState> CollectionState { get; set; } = new Dictionary<string, SyncState>();

		public TokenStatus TokenStatus
		{
			get => _tokenStatus;
			set
			{
				if(value == _tokenStatus)
					return;
				var prev = _tokenStatus;
				_tokenStatus = value;
				TokenStatusChanged?.Invoke(new ValueChangedEventArgs<TokenStatus>(prev, value));
			}
		}

		public string DataDirectory { get; set; }

		public void Save()
		{
			Serializer.Save(this);
		}

		internal void Update(int id, string username)
		{
			Id = id;
			Username = username;
			LastUpdated = DateTime.Now;
			Save();
		}

		internal void Reset()
		{
			UploadToken = string.Empty;
			TokenStatus = TokenStatus.Unclaimed;
			CollectionState.Clear();
			Update(0, null);
		}

		public event Action<ValueChangedEventArgs<TokenStatus>> TokenStatusChanged;

		public class SyncState
		{
			public SyncState(int hash)
			{
				Hash = hash;
				Date = DateTime.Now;
			}

			public SyncState()
			{
			}

			public DateTime Date { get; set; }
			public int Hash { get; set; }
		}
	}

	public enum AccountStatus
	{
		Anonymous,
		Registered
	}
}
