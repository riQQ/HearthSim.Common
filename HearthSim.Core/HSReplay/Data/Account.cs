using System;
using HearthSim.Core.Util;

namespace HearthSim.Core.HSReplay.Data
{
	public class Account : IJsonSerializable
	{
		public static JsonSerializer<Account> Serializer { get; } = new JsonSerializer<Account>("hsreplay.cache", false);

		public string UploadToken { get; set; }
		public AccountStatus Status { get; set; }
		public string Username { get; set; }
		public int Id { get; set; }
		public DateTime LastUpdated { get; set; }

		public string DataDirectory { get; set; }
		public void Save() => Serializer.Save(this);
	}

	public enum AccountStatus
	{
		Anonymous,
		Registered
	}
}
