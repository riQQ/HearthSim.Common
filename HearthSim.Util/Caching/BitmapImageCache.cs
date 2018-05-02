using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HearthSim.Util.Logging;

namespace HearthSim.Util.Caching
{
	public abstract class BitmapImageCache : PersistentWebCache<BitmapImage>
	{
		protected BitmapImageCache(int memCacheSize, string persistentDirectory, Func<string, string> keyToUrl)
			: base(memCacheSize, persistentDirectory, keyToUrl)
		{
			if(!Uri.TryCreate(persistentDirectory, UriKind.RelativeOrAbsolute, out var uri) || uri.Scheme != Uri.UriSchemeFile)
				throw new ArgumentException("Invalid directory");
		}

		public override async Task<BitmapImage> LoadFromDisk(string filePath)
		{
			return await Task.Run(() => LoadImage(filePath));
		}

		private BitmapImage LoadImage(string filePath)
		{
			var bitmapImage = new BitmapImage();
			try
			{
				using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					bitmapImage.BeginInit();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.StreamSource = stream;
					bitmapImage.EndInit();
					bitmapImage.Freeze();
				}
			}
			catch(FileFormatException e)
			{
				Log.Error(e);
				try
				{
					File.Delete(filePath);
					//TODO: Implement retry
				}
				catch(Exception)
				{
				}
			}
			catch(IOException e)
			{
				Log.Error(e);
			}
			return bitmapImage;
		}
	}
}
