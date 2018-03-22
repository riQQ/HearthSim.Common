﻿using System;

namespace HearthSim.Core.HSReplay
{
	public class HSReplayNetEvents
	{
		internal HSReplayNetEvents()
		{
		}

		public event Action CollectionAlreadyUpToDate;
		public event Action CollectionUploaded;
		public event Action CollectionUploadThrottled;
		public event Action<string> CollectionUploadError;
		public event Action<BlizzardAccountClaimEventArgs> BlizzardAccountClaimed;
		public event Action<BlizzardAccountClaimEventArgs> BlizzardAccountClaimError;

		internal void OnCollectionAlreadyUpToDate()
			=> CollectionAlreadyUpToDate?.Invoke();

		internal void OnCollectionUploaded()
			=> CollectionUploaded?.Invoke();

		internal void OnCollectionUploadThrottled()
			=> CollectionUploadThrottled?.Invoke();

		internal void OnCollectionUploadError(string error)
			=> CollectionUploadError?.Invoke(error);

		internal void OnBlizzardAccountClaimed(ulong hi, ulong lo, string battleTag)
			=> BlizzardAccountClaimed?.Invoke(new BlizzardAccountClaimEventArgs(hi, lo, battleTag));

		internal void OnBlizzardAccountClaimError(ClaimError error, ulong hi, ulong lo, string battleTag)
			=> BlizzardAccountClaimed?.Invoke(new BlizzardAccountClaimEventArgs(error, hi, lo, battleTag));
	}
}
