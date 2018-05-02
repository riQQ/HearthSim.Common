using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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

		public override async Task<BitmapImage> LoadFromDisk(string key, string filePath)
		{
			return await Task.Run(() => LoadImage(key, filePath));
		}

		private BitmapImage LoadImage(string key, string filePath)
		{
			BitmapImage bitmapImage = null;
			if(Fetching.Contains(key))
				return new BitmapImage();
			try
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					bitmapImage = new BitmapImage();
					using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = stream;
						bitmapImage.EndInit();
						bitmapImage.Freeze();
					}
				});
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
