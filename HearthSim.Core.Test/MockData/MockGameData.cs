using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using Card = HearthMirror.Objects.Card;

namespace HearthSim.Core.Test.MockData
{
	[Serializable]
	public class MockGameData : IGameDataProvider
	{
		private readonly MatchInfo _matchinfo;
		private readonly GameServerInfo _serverInfo;
		private readonly AccountId _accountId;
		private readonly ArenaInfo _arenaInfo;
		private readonly BattleTag _battleTag;
		private readonly BrawlInfo _brawlInfo;
		private readonly List<Card> _collection;
		private readonly Card[] _draftChoices;
		private readonly List<Card> _packCards;
		private readonly int _packId;
		private readonly DungeonInfo _dungeonInfo;

		public MockGameData(MatchInfo matchinfo = null, GameServerInfo serverInfo = null, AccountId accountId = null,
			ArenaInfo arenaInfo = null, BattleTag battleTag = null, BrawlInfo brawlInfo = null,
			List<Card> collection = null, Card[] draftChoices = null,
			List<Card> packCards = null, int packId = 0, DungeonInfo dungeonInfo = null)
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
		}

		public MatchInfo GetMatchInfo() => _matchinfo;
		public GameServerInfo GetServerInfo() => _serverInfo;
		public AccountId GetAccountId() => _accountId;
		public ArenaInfo GetArenaInfo() => _arenaInfo;
		public BattleTag GetBattleTag() => _battleTag;
		public BrawlInfo GetBrawlInfo() => _brawlInfo;
		public List<Card> GetCollection() => _collection;
		public Card[] GetDraftChoices() => _draftChoices;
		public List<Card> GetPackCards() => _packCards;
		public int GetPackId() => _packId;
		public DungeonInfo GetDungeonInfo() => _dungeonInfo;
		public void Reset()
		{
		}
	}
}
