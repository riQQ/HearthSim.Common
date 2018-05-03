using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HearthSim.Util.Caching
{
	public abstract class Cache<T>
	{
		private readonly int _size;
		private readonly Dictionary<string, T> _cache;
		private readonly LinkedList<string> _keys;

		protected Cache(int size)
		{
			_size = size;
			_cache = new Dictionary<string, T>();
			_keys = new LinkedList<string>();
		}

		protected abstract Task<T> Fetch(string key);

		public async Task<T> Get(string key)
		{
			if(TryGet(key, out var obj))
				return obj;
			obj = await Fetch(key);
			if(obj == null)
				return default(T);
			Add(key, obj);
			return obj;
		}

		public bool TryGet(string key, out T obj)
		{
			if(!_cache.TryGetValue(key, out obj))
			{
				obj = GetPlaceholder(key);
				return false;
			}
			_keys.Remove(key);
			_keys.AddLast(key);
			return true;
		}

		protected virtual T GetPlaceholder(string key)
		{
			return default(T);
		}

		protected void Add(string key, T obj)
		{
			if(_keys.Count >= _size)
				Trim();
			var node = new LinkedListNode<string>(key);
			_keys.AddLast(node);
			_cache[key] = obj;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void Trim()
		{
			_cache.Remove(_keys.First.Value);
			_keys.RemoveFirst();
		}
	}
}
