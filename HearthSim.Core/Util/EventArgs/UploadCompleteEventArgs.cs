using HearthSim.Core.HSReplay;
using HSReplay;

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadCompleteEventArgs : System.EventArgs
	{
		public UploadCompleteEventArgs(UploadMetaData data, LogUploader.UploadStatus status)
		{
			Data = data;
			Status = status;
		}

		public UploadMetaData Data { get; }
		public LogUploader.UploadStatus Status { get; }
	}
}
