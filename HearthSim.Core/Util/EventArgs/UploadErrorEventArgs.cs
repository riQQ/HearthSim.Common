using HSReplay;

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadErrorEventArgs : UploadStatusChangedEventArgs
	{
		public string Reason { get; }

		public UploadErrorEventArgs(int uploadId ,UploadMetaData data, string reason)
			:base(uploadId, data)
		{
			Reason = reason;
		}
	}
}
