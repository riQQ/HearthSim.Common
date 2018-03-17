using System.Collections.Generic;

namespace HearthSim.Core.Util.EventArgs
{
	public class CollectionChangedEventArgs<T> : System.EventArgs
	{
		public CollectionChangedEventArgs(IEnumerable<T> items, IEnumerable<T> added = null,
			IEnumerable<T> removed = null, IEnumerable<T> edited = null)
		{
			Items = items;
			Edited = edited ?? new List<T>();
			Added = added ?? new List<T>();
			Removed = removed ?? new List<T>();
		}

		public IEnumerable<T> Items { get; }
		public IEnumerable<T> Edited { get; }
		public IEnumerable<T> Added { get; }
		public IEnumerable<T> Removed { get; }
	}
}
