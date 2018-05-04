using System.Threading.Tasks;

namespace HearthSim.Util.Caching
{
	public class MemCache<T> : Cache<T>
	{
		public MemCache(int size) : base(size)
		{
			
		}
		protected override Task<(bool, T)> Fetch(string key)
		{
			return null;
		}

		public void Cache(string key, T obj)
		{
			Add(key, obj);
		}
	}
}
