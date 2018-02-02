using System;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Util.EventArgs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class LoadingScreenParserTests
	{
		[TestMethod]
		public void ModeChanged()
		{
			ModeChangedEventArgs modeArgs = null;
			var parser = new LoadingScreenParser();
			parser.ModeChanged += args => modeArgs = args;
			parser.Parse(new Line("LoadingScreen", "D 12:09:10.8365733 LoadingScreen.OnSceneLoaded() - prevMode=GAMEPLAY currMode=TOURNAMENT"));
			Assert.IsNotNull(modeArgs);
			Assert.AreEqual(Mode.GAMEPLAY, modeArgs.PreviousMode);
			Assert.AreEqual(Mode.TOURNAMENT, modeArgs.CurrentMode);
		}

		[TestMethod]
		public void GameplayStart()
		{
			var invoked = false;
			var parser = new LoadingScreenParser();
			parser.GameplayStart += () => invoked = true;
			parser.Parse(new Line("LoadingScreen", "D 12:04:32.9074527 Gameplay.Start()"));
			Assert.IsTrue(invoked);
		}
	}
}
