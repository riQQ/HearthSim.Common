using HearthMirror.Objects;

namespace HearthSim.Core.Util.Watchers.ArenaWatcher
{
	internal interface IArenaProvider
	{
		ArenaInfo GetArenaInfo();
		Card[] GetDraftChoices();
	}
}
