using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HearthSim.Util.Logging;
using Newtonsoft.Json;

namespace HearthSim.Core.Util
{
	public class JsonSerializer<T> where T : class, IJsonSerializable, new()
	{
		private readonly string _fileName;
		private readonly bool _encrypt;

		public JsonSerializer(string fileName, bool encrypt)
		{
			_fileName = fileName;
			_encrypt = encrypt;
		}

		public bool Save(T instance)
		{
			if(!Directory.Exists(instance.DataDirectory))
				Directory.CreateDirectory(instance.DataDirectory);
			var filePath = Path.Combine(instance.DataDirectory, _fileName);
			var json = JsonConvert.SerializeObject(instance);
			try
			{
				var bytes = Encoding.UTF8.GetBytes(json);
				if(_encrypt)
					bytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
				using(var fs = new FileStream(filePath, FileMode.Create))
					fs.Write(bytes, 0, bytes.Length);
				return true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		public void Delete(T obj)
		{
			var filePath = Path.Combine(obj.DataDirectory, _fileName);
			if(!File.Exists(filePath))
				return;
			try
			{
				File.Delete(filePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public T Load(string directory)
		{
			T obj;
			var filePath = Path.Combine(directory, _fileName);
			if(!File.Exists(filePath))
				obj = new T();
			else
			{
				try
				{
					var bytes = File.ReadAllBytes(filePath);
					if(_encrypt)
						bytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);
					obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes)) ?? new T();
				}
				catch(Exception ex)
				{
					Log.Error(ex);
					obj = new T();
				}
			}
			obj.DataDirectory = directory;
			return obj;
		}
	}

	public interface IJsonSerializable
	{
		[JsonIgnore]
		string DataDirectory { get; set; }

		void Save();
	}
}
