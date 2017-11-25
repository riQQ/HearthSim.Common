using System.Collections.Generic;
using HearthMirror;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.Watchers.PackWatcher
{
	internal class HearthMirrorPackProvider : IPackProvider
	{
		public List<Card> GetCards() => Reflection.GetPackCards();
		public int GetPackId() => Reflection.GetLastOpenedBoosterId();
	}
}
