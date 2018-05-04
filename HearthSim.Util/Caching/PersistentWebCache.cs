using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HearthSim.Util.Logging;

namespace HearthSim.Util.Caching
{
	public abstract class PersistentWebCache<T> : Cache<T>
	{
		private readonly string _persistentDirectory;
		private readonly Func<string, string> _keyToUrl;
		protected readonly List<string> Fetching;

		protected PersistentWebCache(int memCacheSize, string persistentDirectory, Func<string, string> keyToUrl) : base(memCacheSize)
		{
			_persistentDirectory = persistentDirectory;
			_keyToUrl = keyToUrl;
			Fetching = new List<string>();
		}

		protected override async Task<(bool, T)> Fetch(string key)
		{
			if(Fetching.Contains(key))
				return (false, default(T));
			Fetching.Add(key);
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
					Fetching.Remove(key);
					Log.Error(e);
					return (false, default(T));
				}
			}
			Fetching.Remove(key);
			return (true, await LoadFromDisk(key, filePath));
		}

		public abstract Task<T> LoadFromDisk(string key, string filePath);
	}
}
