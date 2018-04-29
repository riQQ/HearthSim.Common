using HearthSim.Core.HSReplay;
using HSReplay;

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadCompleteEventArgs : UploadStatusChangedEventArgs
	{
		public UploadCompleteEventArgs(int uploadId, UploadMetaData data, LogUploader.UploadStatus status)
			: base(uploadId, data)
		{
			Status = status;
		}

		public LogUploader.UploadStatus Status { get; }
	}
}
