using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthSim.Core.Hearthstone
{
	public interface IGameDataProvider
	{
		MatchInfo GetMatchInfo();
		GameServerInfo GetServerInfo();
		ArenaInfo GetArenaInfo();
		HearthMirror.Objects.Card[] GetDraftChoices();
		List<HearthMirror.Objects.Card> GetPackCards();
		int GetPackId();
		List<HearthMirror.Objects.Card> GetCollection();
		AccountId GetAccountId();
		BattleTag GetBattleTag();
		BrawlInfo GetBrawlInfo();
		void Reset();
	}
}