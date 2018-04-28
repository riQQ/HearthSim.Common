using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util.EventArgs;
using Deck = HearthSim.Core.Hearthstone.Deck;
using DeckType = HearthSim.Core.Hearthstone.Enums.DeckType;

namespace HearthSim.Core.Util.Watchers
{
	public class DungeonRunWatcher : Watcher
	{
		private readonly IDungeonRunDataProvider _dataProvider;

		private readonly string[] _initialOpponents =
		{
			CardIds.NonCollectible.Rogue.BinkTheBurglarHeroic,
			CardIds.NonCollectible.Hunter.GiantRatHeroic,
			CardIds.NonCollectible.Hunter.WeeWhelpHeroic,

			CardIds.NonCollectible.Druid.AMangyWolfHeroic,
			CardIds.NonCollectible.Hunter.GobblesHeroic,
			CardIds.NonCollectible.Druid.RottoothHeroic,
		};

		private List<int>[] _prevCards;
		private int[] _prevLootChoice;
		private int[] _prevTreasureChoice;

		public DungeonRunWatcher(IDungeonRunDataProvider dataProvider, int updateDelay = 500) : base(updateDelay)
		{
			_dataProvider = dataProvider;
		}

		public event Action<DungeonRunDeckUpdatedEventArgs> DungeonRunDeckUpdated;
		public event Action<DungeonRunMatchStartedEventArgs> DungeonRunMatchStarted;

		protected override void Reset()
		{
			_prevCards = new List<int>[] { null, null };
			_prevLootChoice = new[] { 0, 0 };
			_prevTreasureChoice = new[] { 0, 0 };
		}

		public override UpdateResult Update()
		{
			if(_dataProvider.InAdventureScreen)
			{
				var dungeonInfo = _dataProvider.GetDungeonInfo();
				if(dungeonInfo != null)
				{
					for(var i = 0; i < dungeonInfo.Length; i++)
					{
						if(dungeonInfo[i]?.RunActive ?? false)
						{
							if(_prevCards[i] == null || !dungeonInfo[i].DbfIds.SequenceEqual(_prevCards[i])
													 || _prevLootChoice[i] != dungeonInfo[i].PlayerChosenLoot
													 || _prevTreasureChoice[i] != dungeonInfo[i].PlayerChosenTreasure)
							{
								_prevCards[i] = dungeonInfo[i].DbfIds.ToList();
								_prevLootChoice[i] = dungeonInfo[i].PlayerChosenLoot;
								_prevTreasureChoice[i] = dungeonInfo[i].PlayerChosenTreasure;
								DungeonRunDeckUpdated?.Invoke(new DungeonRunDeckUpdatedEventArgs(BuildDeck(dungeonInfo[i])));
							}
						}
						else
							_prevCards[i] = null;
					}

					if(_prevLootChoice.All(x => x > 0) && _prevTreasureChoice.All(x => x > 0))
						return UpdateResult.Break;
				}
				else
					_prevCards = new List<int>[] { null, null };
			}
			else if(_dataProvider.InAiMatch && !string.IsNullOrEmpty(_dataProvider.OpponentHeroId))
			{
				if(Cards.All.TryGetValue(_dataProvider.OpponentHeroId, out var card))
				{
					if(DungeonRun.IsDungeonBoss(card))
					{
						var newRun = _initialOpponents.Contains(_dataProvider.OpponentHeroId);
						var deck = newRun ? DungeonRun.GetDefaultDeck(_dataProvider.LocalPlayerClass, card.Set) : null;
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
			return new Deck(DeckType.DungeonRun, "Dungeon Run", (CardClass)info.HeroCardClass, allCards);
		}

		public interface IDungeonRunDataProvider
		{
			bool InAiMatch { get; }
			bool InAdventureScreen { get; }
			string OpponentHeroId { get; }
			CardClass LocalPlayerClass { get; }
			DungeonInfo[] GetDungeonInfo();
		}
	}
}
