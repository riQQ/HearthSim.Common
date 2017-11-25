using HearthMirror;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.Watchers.ArenaWatcher
{
	internal class HearthMirrorArenaProvider : IArenaProvider
	{
		public ArenaInfo GetArenaInfo() => Reflection.GetArenaDeck();
		public Card[] GetDraftChoices() => Reflection.GetArenaDraftChoices()?.ToArray();
	}
}
