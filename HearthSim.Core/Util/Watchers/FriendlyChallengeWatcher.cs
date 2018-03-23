using System;
using HearthSim.Core.Hearthstone;

namespace HearthSim.Core.Util.Watchers
{
	internal class FriendlyChallengeWatcher : Watcher
	{
		private readonly IGameDataProvider _provider;
		private bool _previousVisibilityValue;
		public event Action FriendlyChallenge;

		public FriendlyChallengeWatcher(IGameDataProvider provider, int delay = 500) : base(delay)
		{
			_provider = provider;
		}

		public override UpdateResult Update()
		{
			var dialogVisible = _provider.FriendlyChallengeDialogVisible();
			if(dialogVisible != _previousVisibilityValue)
			{
				if(dialogVisible)
					FriendlyChallenge?.Invoke();
				_previousVisibilityValue = dialogVisible;
			}
			return UpdateResult.Continue;
		}
	}
}
