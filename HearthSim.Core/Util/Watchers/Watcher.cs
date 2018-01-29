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

		protected Watcher(int updateDelay = 500)
		{
			UpdateDelay = updateDelay;
		}

		public int UpdateDelay { get; }

		public bool IsRunning { get; private set; }

		/// <summary>
		///     Resets variables and starts the watcher, if not already running.
		/// </summary>
		public void Run()
		{
			if(IsRunning)
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
			while(IsRunning)
				await Task.Delay(UpdateDelay);
		}

		private async void UpdateAsync()
		{
			IsRunning = true;
			while(_watch)
			{
				await Task.Delay(UpdateDelay);
				if(!_watch)
					break;
				if(Update() == UpdateResult.Break)
					break;
			}

			IsRunning = false;
		}

		/// <summary>
		///     Update function called every x milliseconds (updateDelay set in ctor).
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
