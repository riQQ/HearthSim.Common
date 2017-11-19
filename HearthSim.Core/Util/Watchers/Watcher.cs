using System.Threading.Tasks;

namespace HearthSim.Core.Util.Watchers
{
	public abstract class Watcher
	{
		protected Watcher(int delay = 500)
		{
			Delay = delay;
		}

		protected int Delay { get; }
		protected bool Running { get; private set; }
		private bool _watch;

		public void Run()
		{
			if(Running)
				return;
			_watch = true;
			Reset();
			UpdateAsync();
		}

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
				Update();
				await Task.Delay(Delay);
			}
			Running = false;
		}

		public abstract void Update();

		protected virtual void Reset()
		{
		}
	}
}
