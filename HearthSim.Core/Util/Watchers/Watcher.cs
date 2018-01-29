using System.Threading.Tasks;

namespace HearthSim.Core.Util.Watchers
{
	public abstract class Watcher
	{
		public enum UpdateResult
		{
			Continue,
			Break
		}

		private bool _watch;

		protected Watcher(int delay = 500)
		{
			Delay = delay;
		}

		protected int Delay { get; }
		protected bool Running { get; private set; }

		/// <summary>
		///     Resets variables and starts the watcher, if not already running.
		/// </summary>
		public void Run()
		{
			if(Running)
				return;
			_watch = true;
			Reset();
			UpdateAsync();
		}

		/// <summary>
		///     Stops the watcher.
		/// </summary>
		public async Task Stop()
		{
			_watch = false;
			while(Running)
				await Task.Delay(Delay);
		}

		private async void UpdateAsync()
		{
			Running = true;
			while(_watch)
			{
				await Task.Delay(Delay);
				if(!_watch)
					break;
				if(Update() == UpdateResult.Break)
					break;
			}
			Running = false;
		}

		/// <summary>
		///     Update function called every x milliseconds (delay set in ctor).
		/// </summary>
		/// <returns>Watcher should stop</returns>
		public abstract UpdateResult Update();

		/// <summary>
		///     Optional variable reset, called before the first Update on Run().
		/// </summary>
		protected virtual void Reset()
		{
		}
	}
}
