using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Util.Watchers.PackWatcher
{
	internal class PackWatcher
	{
		private readonly List<Card> _previousPack = new List<Card>();
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private bool _invokeEvent;
		private readonly IPackProvider _packProvider;

		public PackWatcher(IPackProvider packProvider, int delay = 500)
		{
			_packProvider = packProvider ?? throw new ArgumentNullException(nameof(packProvider));
			_delay = delay;
		}
		public event Action<PackOpenedEventArgs> PackOpened;

		public void Run()
		{
			_watch = true;
			if(!_running)
				CheckForPacks();
		}

		public void Stop() => _watch = false;

		private async void CheckForPacks()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var cards = _packProvider.GetCards();
				if(cards?.Count == 5)
				{
					if(cards.All(x => _previousPack.Any(c => c.Id == x.Id & c.Premium == x.Premium)))
						continue;
					if(_previousPack.Any())
						_invokeEvent = true;
					_previousPack.Clear();
					_previousPack.AddRange(cards);
					if(_invokeEvent)
						PackOpened?.Invoke(new PackOpenedEventArgs(cards, _packProvider.GetPackId()));
				}
				else
					_invokeEvent = true;
			}
			_running = false;
		}

	}
}
