using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;

namespace HearthSim.Core.Util.Watchers
{
	public class DungeonRunWatcher : Watcher
	{
		private readonly IDungeonRunDataProvider _dataProvider;

		private readonly string[] _initialOpponents =
		{
			CardIds.NonCollectible.Rogue.BinkTheBurglarHeroic,
			CardIds.NonCollectible.Hunter.GiantRatHeroic,
			CardIds.NonCollectible.Hunter.WeeWhelpHeroic
		};

		private List<int> _prevCards;
		private int _prevLootChoice;
		private int _prevTreasureChoice;

		public DungeonRunWatcher(IDungeonRunDataProvider dataProvider, int updateDelay = 500) : base(updateDelay)
		{
			_dataProvider = dataProvider;
		}

		public event Action<DungeonInfo> DungeonInfoChanged;
		public event Action<RunType> DungeonRunMatchStarted;

		protected override void Reset()
		{
			_prevCards = null;
			_prevLootChoice = 0;
			_prevTreasureChoice = 0;
		}

		public override UpdateResult Update()
		{
			if(_dataProvider.InAdventureScreen)
			{
				var dungeonInfo = _dataProvider.GetDungeonInfo();
				if(dungeonInfo?.RunActive ?? false)
				{
					if(_prevCards == null || !dungeonInfo.DbfIds.SequenceEqual(_prevCards)
										|| _prevLootChoice != dungeonInfo.PlayerChosenLoot
										|| _prevTreasureChoice != dungeonInfo.PlayerChosenTreasure)
					{
						_prevCards = dungeonInfo.DbfIds.ToList();
						_prevLootChoice = dungeonInfo.PlayerChosenLoot;
						_prevTreasureChoice = dungeonInfo.PlayerChosenTreasure;
						DungeonInfoChanged?.Invoke(dungeonInfo);
						if(_prevLootChoice > 0 && _prevTreasureChoice > 0)
							return UpdateResult.Break;
					}
				}
				else
					_prevCards = null;
			}
			else if(_dataProvider.InAiMatch && !string.IsNullOrEmpty(_dataProvider.OpponentHeroId))
			{
				if(Cards.All.TryGetValue(_dataProvider.OpponentHeroId, out var card))
				{
					if(card.Set == CardSet.LOOTAPALOOZA && card.Id.Contains("BOSS"))
					{
						var newRun = _initialOpponents.Contains(_dataProvider.OpponentHeroId);
						DungeonRunMatchStarted?.Invoke(newRun ? RunType.New : RunType.Existing);
						return UpdateResult.Break;
					}
				}
			}

			return UpdateResult.Continue;
		}

		public enum RunType
		{
			New,
			Existing
		}

		public interface IDungeonRunDataProvider
		{
			bool InAiMatch { get; }
			bool InAdventureScreen { get; }
			string OpponentHeroId { get; }
			DungeonInfo GetDungeonInfo();
		}
	}
}
