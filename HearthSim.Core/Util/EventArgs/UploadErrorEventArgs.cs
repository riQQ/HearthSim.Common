#region




#endregion

namespace HearthSim.Core.Util.EventArgs
{
	public class UploadErrorEventArgs : System.EventArgs
	{
		public string Reason { get; }

		public UploadErrorEventArgs(string reason)
		{
			Reason = reason;
		}
	}
}
