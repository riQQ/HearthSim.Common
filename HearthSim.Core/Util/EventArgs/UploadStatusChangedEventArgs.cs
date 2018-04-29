using HSReplay;

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadStatusChangedEventArgs : System.EventArgs
	{
		public int UploadId { get; }
		public UploadMetaData Data { get; }

		public UploadStatusChangedEventArgs(int uploadId, UploadMetaData data)
		{
			UploadId = uploadId;
			Data = data;
		}
	}
}
