using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthSim.Util.Logging;

namespace HearthSim.Util.Caching
{
	public abstract class PersistentWebCache<T> : Cache<T>
	{
		private readonly string _persistentDirectory;
		private readonly Func<string, string> _keyToUrl;

		protected PersistentWebCache(int memCacheSize, string persistentDirectory, Func<string, string> keyToUrl) : base(memCacheSize)
		{
			_persistentDirectory = persistentDirectory;
			_keyToUrl = keyToUrl;
		}

		protected override async Task<T> Fetch(string key)
		{
			var url = _keyToUrl(key);
			var fileName = url.Split('/').Last();
			var filePath = Path.Combine(_persistentDirectory, fileName);
			if(!File.Exists(filePath))
			{
				try
				{
					Directory.CreateDirectory(_persistentDirectory);
					using(var webClient = new WebClient())
						await webClient.DownloadFileTaskAsync(url, filePath);
				}
				catch(Exception e)
				{
					Log.Error(e);
					return default(T);
				}
			}
			return await LoadFromDisk(filePath);
		}

		public abstract Task<T> LoadFromDisk(string filePath);
	}
}
