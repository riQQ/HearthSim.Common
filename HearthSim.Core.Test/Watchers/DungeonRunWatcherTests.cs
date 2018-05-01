
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Watchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.Watchers
{
	[TestClass]
	public class DungeonRunWatcherTests
	{
		private const int BattleTotem = 47300;
		private const int PotionOfVitality = 46405;
		private const int CrystalGem = 46407;
		private const int Backstab = 180;
		private const int DeadlyPoison = 459;
		private const int PitSnake = 2882;
		private const int SinisterStrike = 710;
		private const int Category = -1;

		private DungeonRunMatchStartedEventArgs _run;
		private DungeonRunDeckUpdatedEventArgs _info;
		private MockDungeonRunData _data;
		private DungeonRunWatcher _watcher;

		[TestInitialize]
		public void Initialize()
		{
			_run = null;
			_info = null;
			_data = new MockDungeonRunData();
			_watcher = new DungeonRunWatcher(_data, 100);
			_watcher.DungeonRunMatchStarted += run => _run = run;
			_watcher.DungeonRunDeckUpdated += info => _info = info;
		}

		[TestMethod]
		public void Update_Noop()
		{
			var result = _watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
			Assert.IsNull(_run);
			Assert.IsNull(_info);
		}

		[TestMethod]
		public void InAdventureScreen_NoData_Noop()
		{
			_data.InAdventureScreen = true;
			var result = _watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunInactive_Noop()
		{
			_data.InAdventureScreen = true;
			var result = _watcher.Update();
			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = false
				},
				new MockDungeonInfo
				{
					RunActive = false
				}
			};
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunActiveNotChosen_NewData()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> {1, 2, 3},
					LootA = new List<int> { 1 },
					LootB = new List<int> { 2 },
					LootC = new List<int> { 3 },
					Treasure = new List<int> { 1, 2, 3 },
				},
				new MockDungeonInfo
				{
					RunActive = false,
				}
			};
			var result = _watcher.Update();
			Assert.IsNull(_info);
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunActiveChosen_NewData()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { Backstab },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					Treasure = new List<int> { BattleTotem, PotionOfVitality, CrystalGem },
					PlayerChosenLoot = 1,
					PlayerChosenTreasure = 1
				},
				new MockDungeonInfo
				{
					RunActive = false,
				}
			};

			var result = _watcher.Update();
			Assert.IsNotNull(_info);

			var dbfIds = _info.Deck.Cards.Select(x => x.Data?.DbfId ?? 0).OrderBy(x => x).ToArray();
			var expectedDbfIds = new[] {Backstab, BattleTotem, DeadlyPoison}.OrderBy(x => x);
			Assert.IsTrue(expectedDbfIds.SequenceEqual(dbfIds), $"Found: {(string.Join(", ", dbfIds))}");
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);

			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { Backstab },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					Treasure = new List<int> { BattleTotem, PotionOfVitality, CrystalGem },
					PlayerChosenLoot = 1,
					PlayerChosenTreasure = 1
				},
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { PitSnake },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					Treasure = new List<int> { BattleTotem, PotionOfVitality, CrystalGem },
					PlayerChosenLoot = 1,
					PlayerChosenTreasure = 1
				}
			};

			_info = null;
			result = _watcher.Update();
			Assert.IsNotNull(_info);

			dbfIds = _info.Deck.Cards.Select(x => x.Data?.DbfId ?? 0).OrderBy(x => x).ToArray();
			expectedDbfIds = new[] {PitSnake, BattleTotem, DeadlyPoison}.OrderBy(x => x);
			Assert.IsTrue(expectedDbfIds.SequenceEqual(dbfIds), $"Found: {(string.Join(", ", dbfIds))}");
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunActiveChosenNoTreasure_NewData()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { Backstab },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					PlayerChosenLoot = 1,
				},
				new MockDungeonInfo
				{
					RunActive = false,
				}
			};

			var result = _watcher.Update();
			Assert.IsNotNull(_info);

			var dbfIds = _info.Deck.Cards.Select(x => x.Data?.DbfId ?? 0).OrderBy(x => x).ToArray();
			var expectedDbfIds = new[] {Backstab, DeadlyPoison}.OrderBy(x => x);
			Assert.IsTrue(expectedDbfIds.SequenceEqual(dbfIds), $"Found: {(string.Join(", ", dbfIds))}");
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);

			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { Backstab },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					PlayerChosenLoot = 1,
				},
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { PitSnake },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					PlayerChosenLoot = 1,
				}
			};

			_info = null;
			result = _watcher.Update();
			Assert.IsNotNull(_info);

			dbfIds = _info.Deck.Cards.Select(x => x.Data?.DbfId ?? 0).OrderBy(x => x).ToArray();
			expectedDbfIds = new[] {PitSnake, DeadlyPoison}.OrderBy(x => x);
			Assert.IsTrue(expectedDbfIds.SequenceEqual(dbfIds), $"Found: {(string.Join(", ", dbfIds))}");
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}

		[TestMethod]
		public void InAdventureScreen_InfoNotUpdatedUntilReset()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new[]
			{
				new MockDungeonInfo
				{
					RunActive = true,
					DbfIds = new List<int> { Backstab },
					LootA = new List<int> { Category, DeadlyPoison },
					LootB = new List<int> { Category, PitSnake },
					LootC = new List<int> { Category, SinisterStrike },
					Treasure = new List<int> { BattleTotem, PotionOfVitality, CrystalGem },
					PlayerChosenLoot = 1,
					PlayerChosenTreasure = 1
				},
				new MockDungeonInfo
				{
					RunActive = false,
				}
			};

			_watcher.Update();
			Assert.IsNotNull(_info);

			_info = null;
			_watcher.Update();
			Assert.IsNull(_info);

			_watcher.Run();
			Thread.Sleep(130);
			Assert.IsNotNull(_info);
		}

		[TestMethod]
		public void InMatch_NoData_Noop()
		{
			_data.InAiMatch = true;
			var result = _watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InMatch_NonBossHero_Noop()
		{
			_data.InAiMatch = true;
			_data.OpponentHeroId = "HERO_01";
			var result = _watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InMatch_InitialBossHero_NewRun()
		{
			_data.InAiMatch = true;
			_data.LocalPlayerClass = CardClass.MAGE;
			_data.OpponentHeroId = CardIds.NonCollectible.Hunter.GiantRatHeroic;
			var result = _watcher.Update();
			Assert.IsNotNull(_run);
			Assert.IsTrue(_run.IsNew);
			Assert.IsNotNull(_run.Deck);
			Assert.AreEqual(10, _run.Deck.Cards.Count());
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}

		[TestMethod]
		public void InMatch_LateBossHero_NewRun()
		{
			_data.InAiMatch = true;
			_data.LocalPlayerClass = CardClass.MAGE;
			_data.OpponentHeroId = CardIds.NonCollectible.Hunter.TrappedRoomHeroic;
			var result = _watcher.Update();
			Assert.IsNotNull(_run);
			Assert.IsFalse(_run.IsNew);
			Assert.IsNull(_run.Deck);
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}
	}

	public class MockDungeonRunData : DungeonRunWatcher.IDungeonRunDataProvider
	{
		public bool InAiMatch { get; set; }
		public bool InAdventureScreen { get; set; }
		public string OpponentHeroId { get; set; }
		public CardClass LocalPlayerClass { get; set; }
		public MockDungeonInfo[] MockDungeonInfo { get; set; }
		public DungeonInfo[] GetDungeonInfo()
		{
			return MockDungeonInfo;
		}
	}

	public class MockDungeonInfo : DungeonInfo
	{
		public new bool RunActive
		{
			set => _runActive = value;
		}

		public new List<int> DbfIds
		{
			set => _dbfIds = value;
		}

		public new int PlayerChosenLoot
		{
			set => _playerChosenLoot = value;
		}

		public new int PlayerChosenTreasure
		{
			set => _playerChosenTreasure = value;
		}

		public new List<int> LootA
		{
			set => _lootA = value;
		}

		public new List<int> LootB
		{
			set => _lootB = value;
		}

		public new List<int> LootC
		{
			set => _lootC = value;
		}

		public new List<int> Treasure
		{
			set => _treasure = value;
		}
	}
}
