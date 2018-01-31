﻿using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util.EventArgs;
using Deck = HearthSim.Core.Hearthstone.Deck;

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

		public event Action<DungeonRunDeckUpdatedEventArgs> DungeonRunDeckUpdated;
		public event Action<DungeonRunMatchStartedEventArgs> DungeonRunMatchStarted;

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
						if(_prevLootChoice > 0 && (_prevTreasureChoice > 0 || dungeonInfo.Treasure == null))
						{
							DungeonRunDeckUpdated?.Invoke(new DungeonRunDeckUpdatedEventArgs(BuildDeck(dungeonInfo)));
							return UpdateResult.Break;
						}
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
						var deck = newRun ? DungeonRun.GetDefaultDeck(_dataProvider.LocalPlayerClass) : null;
						DungeonRunMatchStarted?.Invoke(new DungeonRunMatchStartedEventArgs(newRun, deck));
						return UpdateResult.Break;
					}
				}
			}

			return UpdateResult.Continue;
		}

		private Deck BuildDeck(DungeonInfo info)
		{
			var allCards = info.DbfIds.ToList();
			if(info.PlayerChosenLoot > 0)
			{
				var loot = new[] { info.LootA, info.LootB, info.LootC };
				var chosen = loot[info.PlayerChosenLoot - 1];
				for(var i = 1; i < chosen.Count; i++)
					allCards.Add(chosen[i]);
			}
			if(info.PlayerChosenTreasure > 0)
				allCards.Add(info.Treasure[info.PlayerChosenTreasure - 1]);
			return new Deck("Dungeon Run", (CardClass)info.HeroCardClass, allCards);
		}

		public interface IDungeonRunDataProvider
		{
			bool InAiMatch { get; }
			bool InAdventureScreen { get; }
			string OpponentHeroId { get; }
			CardClass LocalPlayerClass { get; }
			DungeonInfo GetDungeonInfo();
		}
	}
}
