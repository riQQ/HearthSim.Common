using System;
using System.IO;
using HearthSim.Util.Logging;

namespace HearthSim.Core.HSReplay
{
	internal class UploadTokenHistory
	{
		private readonly string _directory;

		public UploadTokenHistory(string directory)
		{
			_directory = directory;
		}

		public void Write(string data)
		{
			data = DateTime.Now.ToShortDateString() + " " 
				+ DateTime.Now.ToShortTimeString() + ": " + data;
			try
			{
				var file = Path.Combine(_directory, "upload_token_history.txt");
				File.AppendAllLines(file, new[] {data});
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
