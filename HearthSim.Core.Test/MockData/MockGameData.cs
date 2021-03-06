﻿using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using Card = HearthMirror.Objects.Card;
using Collection = HearthMirror.Objects.Collection;

namespace HearthSim.Core.Test.MockData
{
	[Serializable]
	public class MockGameData : IGameDataProvider
	{
		protected MatchInfo _matchinfo;
		protected GameServerInfo _serverInfo;
		protected AccountId _accountId;
		protected ArenaInfo _arenaInfo;
		protected BattleTag _battleTag;
		protected BrawlInfo _brawlInfo;
		protected Collection _collection;
		protected Card[] _draftChoices;
		protected List<Card> _packCards;
		protected int _packId;
		protected DungeonInfo[] _dungeonInfo;
		private readonly bool _friendlyChallengeDialogVisible;

		public MockGameData(MatchInfo matchinfo = null, GameServerInfo serverInfo = null, AccountId accountId = null,
			ArenaInfo arenaInfo = null, BattleTag battleTag = null, BrawlInfo brawlInfo = null,
			Collection collection = null, Card[] draftChoices = null, List<Card> packCards = null,
			int packId = 0, DungeonInfo[] dungeonInfo = null, bool friendlyChallengeDialogVisible = false)
		{
			_matchinfo = matchinfo;
			_serverInfo = serverInfo;
			_accountId = accountId;
			_arenaInfo = arenaInfo;
			_battleTag = battleTag;
			_brawlInfo = brawlInfo;
			_collection = collection;
			_draftChoices = draftChoices;
			_packCards = packCards;
			_packId = packId;
			_dungeonInfo = dungeonInfo;
			_friendlyChallengeDialogVisible = friendlyChallengeDialogVisible;
		}

		public MatchInfo GetMatchInfo() => _matchinfo;
		public GameServerInfo GetServerInfo() => _serverInfo;
		public AccountId GetAccountId() => _accountId;
		public ArenaInfo GetArenaInfo() => _arenaInfo;
		public BattleTag GetBattleTag() => _battleTag;
		public BrawlInfo GetBrawlInfo() => _brawlInfo;
		public Collection GetFullCollection() => _collection;
		public Card[] GetDraftChoices() => _draftChoices;
		public List<Card> GetPackCards() => _packCards;
		public int GetPackId() => _packId;
		public DungeonInfo[] GetDungeonInfo() => _dungeonInfo;
		public void Reset()
		{
		}

		public bool FriendlyChallengeDialogVisible() => _friendlyChallengeDialogVisible;
	}
}
