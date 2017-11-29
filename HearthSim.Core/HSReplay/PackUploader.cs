using System;
using System.Linq;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util.Logging;
using HSReplay;

namespace HearthSim.Core.HSReplay
{
	public class PackUploader
	{
		private readonly ApiWrapper _api;

		internal PackUploader(ApiWrapper api)
		{
			_api = api;
		}

		public async void UploadPack(Account account, Pack pack)
		{
			try
			{
				Log.Info($"Uploading new pack: Id={pack.BoosterType}, Cards=[{string.Join(", ", pack.Cards.Select(x => x.Id + (x.Golden > 0 ? " (g)" : "")))}]");
				var payload = GeneratePayload(account, pack);
				if(payload != null)
					await _api.UploadPack(payload);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private PackData GeneratePayload(Account account, Pack pack)
		{
			if(pack.Cards.Count != 5)
			{
				Log.Error("Invalid card count: " + pack.Cards.Count);
				return null;
			}
			if(account == null || account.AccountHi == 0 || account.AccountLo == 0)
			{
				Log.Error("Could not get account id");
				return null;
			}
			var data = new PackData
			{
				AccountHi = account.AccountHi,
				AccountLo = account.AccountLo,
				BoosterType = (int)pack.BoosterType,
				Date = DateTime.Now.ToString("o"),
				Cards = pack.Cards.Select(x => new CardData {CardId = x.Id, Premium = x.Golden > 0}).ToArray()
			};
			return data;
		}
	}
}
