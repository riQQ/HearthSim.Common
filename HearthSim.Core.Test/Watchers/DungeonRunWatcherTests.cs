
using System.Collections.Generic;
using System.Threading;
using HearthDb;
using HearthMirror.Objects;
using HearthSim.Core.Util.Watchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.Watchers
{
	[TestClass]
	public class DungeonRunWatcherTests
	{
		private DungeonRunWatcher.RunType? _run;
		private DungeonInfo _info;
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
			_watcher.DungeonInfoChanged += info => _info = info;
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
			_data.MockDungeonInfo = new MockDungeonInfo
			{
				RunActive = false
			};
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunActiveNotChosen_NewData()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new MockDungeonInfo
			{
				RunActive = true,
				DbfIds = new List<int> {1, 2, 3},
			};
			var result = _watcher.Update();
			Assert.IsNotNull(_info);
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
		}

		[TestMethod]
		public void InAdventureScreen_RunActiveChosen_NewData()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new MockDungeonInfo
			{
				RunActive = true,
				DbfIds = new List<int> {1, 2, 3},
				PlayerChosenLoot = 1,
				PlayerChosenTreasure = 1
			};
			var result = _watcher.Update();
			Assert.IsNotNull(_info);
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}

		[TestMethod]
		public void InAdventureScreen_InfoNotUpdatedUntilReset()
		{
			_data.InAdventureScreen = true;
			_data.MockDungeonInfo = new MockDungeonInfo
			{
				RunActive = true,
				DbfIds = new List<int> {1, 2, 3},
				PlayerChosenLoot = 1,
				PlayerChosenTreasure = 1
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
			_data.OpponentHeroId = CardIds.NonCollectible.Hunter.GiantRatHeroic;
			var result = _watcher.Update();
			Assert.AreEqual(DungeonRunWatcher.RunType.New, _run);
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}

		[TestMethod]
		public void InMatch_LateBossHero_NewRun()
		{
			_data.InAiMatch = true;
			_data.OpponentHeroId = CardIds.NonCollectible.Hunter.TrappedRoomHeroic;
			var result = _watcher.Update();
			Assert.AreEqual(DungeonRunWatcher.RunType.Existing, _run);
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
		}
	}

	public class MockDungeonRunData : DungeonRunWatcher.IDungeonRunDataProvider
	{
		public bool InAiMatch { get; set; }
		public bool InAdventureScreen { get; set; }
		public string OpponentHeroId { get; set; }
		public MockDungeonInfo MockDungeonInfo { get; set; }
		public DungeonInfo GetDungeonInfo()
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
	}
}
