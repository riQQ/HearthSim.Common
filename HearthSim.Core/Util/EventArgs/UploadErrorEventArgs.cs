using HSReplay;

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadErrorEventArgs : System.EventArgs
	{
		public UploadMetaData Data { get; }
		public string Reason { get; }

		public UploadErrorEventArgs(UploadMetaData data, string reason)
		{
			Data = data;
			Reason = reason;
		}
	}
}
