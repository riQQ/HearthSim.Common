using System.IO;

namespace HearthSim.Core.LogReading.Internal
{
	internal static class Util
	{
		public static bool ArchiveFile(string filePath)
		{
			if(!File.Exists(filePath))
				return false;
			try
			{
				File.Move(filePath, filePath);
				var old = filePath.Replace(".log", "_old.log");
				if(File.Exists(old))
				{
					try
					{
						File.Delete(old);
					}
					catch
					{
					}
				}
				File.Move(filePath, old);
				return true;
			}
			catch
			{
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
				return false;
			}
		}
	}
}
