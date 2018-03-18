using System;

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

		internal void OnCollectionAlreadyUpToDate()
			=> CollectionAlreadyUpToDate?.Invoke();

		internal void OnCollectionUploaded()
			=> CollectionUploaded?.Invoke();

		internal void OnCollectionUploadThrottled()
			=> CollectionUploadThrottled?.Invoke();

		internal void OnCollectionUploadError(string error)
			=> CollectionUploadError?.Invoke(error);
	}
}
