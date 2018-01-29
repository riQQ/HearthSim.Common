using System.Threading;
using HearthSim.Core.Util.Watchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.Watchers
{
	[TestClass]
	public class WatcherTests
	{
		[TestMethod]
		public void ManualUpdateCall()
		{
			var watcher = new MockWatcher(100);

			var result = watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Continue, result);
			Assert.AreEqual(1, watcher.UpdateCount);
			Assert.IsFalse(watcher.IsRunning);

			watcher.MockUpdateResult = Watcher.UpdateResult.Break;
			result = watcher.Update();
			Assert.AreEqual(Watcher.UpdateResult.Break, result);
			Assert.AreEqual(2, watcher.UpdateCount);
			Assert.IsFalse(watcher.IsRunning);
		}

		[TestMethod]
		public void Reset()
		{
			var watcher = new MockWatcher(100);
			Assert.AreEqual(0, watcher.UpdateCount);
			watcher.Run();
			Assert.AreEqual(-1, watcher.UpdateCount);
			watcher.Stop().Wait();
		}

		[TestMethod]
		public void StartStop()
		{
			var watcher = new MockWatcher(100);
			Assert.AreEqual(100, watcher.UpdateDelay);
			Assert.IsFalse(watcher.IsRunning);

			watcher.Run();
			Assert.IsTrue(watcher.IsRunning);

			Thread.Sleep(130);
			Assert.AreEqual(0, watcher.UpdateCount);

			watcher.MockUpdateResult = Watcher.UpdateResult.Break;
			Thread.Sleep(130);
			Assert.AreEqual(1, watcher.UpdateCount);
			Assert.IsFalse(watcher.IsRunning);

			Thread.Sleep(130);
			Assert.AreEqual(1, watcher.UpdateCount);
			Assert.IsFalse(watcher.IsRunning);
		}

		public class MockWatcher : Watcher
		{
			public MockWatcher(int updateDelay) : base(updateDelay)
			{
			}

			public int UpdateCount;

			public UpdateResult MockUpdateResult { get; set; } = UpdateResult.Continue;

			public override UpdateResult Update()
			{
				UpdateCount++;
				return MockUpdateResult;
			}

			protected override void Reset()
			{
				UpdateCount = -1;
			}
		}
	}
}
