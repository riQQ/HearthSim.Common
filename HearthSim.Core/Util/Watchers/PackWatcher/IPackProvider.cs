using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.Watchers.PackWatcher
{
	internal interface IPackProvider
	{
		List<Card> GetCards();
		int GetPackId();
	}
}
