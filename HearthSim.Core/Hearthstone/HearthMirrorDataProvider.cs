﻿using System.Collections.Generic;
using HearthMirror;
using HearthMirror.Objects;

namespace HearthSim.Core.Hearthstone
{
	public class HearthMirrorDataProvider : IGameDataProvider
	{
		public MatchInfo GetMatchInfo() => Reflection.GetMatchInfo();
		public GameServerInfo GetServerInfo() => Reflection.GetServerInfo();
		public void Reset() => Reflection.Reinitialize();
		public ArenaInfo GetArenaInfo() => Reflection.GetArenaDeck();
		public HearthMirror.Objects.Card[] GetDraftChoices() => Reflection.GetArenaDraftChoices()?.ToArray();
		public List<HearthMirror.Objects.Card> GetPackCards() => Reflection.GetPackCards();
		public int GetPackId() => Reflection.GetLastOpenedBoosterId();
		public HearthMirror.Objects.Collection GetFullCollection() => Reflection.GetFullCollection();
		public AccountId GetAccountId() => Reflection.GetAccountId();
		public BattleTag GetBattleTag() => Reflection.GetBattleTag();
		public BrawlInfo GetBrawlInfo() => Reflection.GetBrawlInfo();
		public DungeonInfo[] GetDungeonInfo() => Reflection.GetDungeonInfo();
		public bool FriendlyChallengeDialogVisible() => Reflection.IsFriendlyChallengeDialogVisible();
	}
}
