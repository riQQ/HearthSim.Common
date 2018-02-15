using System;
using System.IO;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.LogReading.Data;
using HearthSim.Core.Test.LogTests.TestHelpers;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthSim.Core.Test.LogParsing
{
	[TestClass]
	public class PowerParserTests
	{
		[TestMethod]
		public void CreateGame()
		{
			var createGame = 0;
			var parser = new PowerParser(new MockGameInfo());
			parser.CreateGame += () => createGame++;

			parser.Parse(new Line("Power", "D 00:29:50.5743926 GameState.DebugPrintPower() - CREATE_GAME"));
			Assert.AreEqual(1, createGame);

			parser.Parse(new Line("Power", "D 00:29:52.6023387 PowerTaskList.DebugPrintPower() -     CREATE_GAME"));
			Assert.AreEqual(1, createGame);
		}

		[TestMethod]
		public void GameStateChange_GameEntity()
		{
			FullEntity entity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => entity = args as FullEntity;

			parser.Parse(new Line("Power", "D 00:29:52.6063231 PowerTaskList.DebugPrintPower() -         GameEntity EntityID=1"));
			Assert.IsNotNull(entity);
			var data = entity.Data as GameEntityData;
			Assert.IsNotNull(data);
			Assert.AreEqual(1, data.Id);
		}

		[TestMethod]
		public void GameStateChange_PlayerEntity()
		{
			FullEntity entity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => entity = args as FullEntity;

			parser.Parse(new Line("Power", "D 00:29:52.6665096 PowerTaskList.DebugPrintPower() -         Player EntityID=3 PlayerID=2 GameAccountId=[hi=144115198130930503 lo=15856412]"));
			Assert.IsNotNull(entity);
			var data = entity.Data as PlayerEntityData;
			Assert.IsNotNull(data);
			Assert.AreEqual(2, data.PlayerId);
			Assert.AreEqual(3, data.Id);
		}

		[TestMethod]
		public void GameStateChange_Entity()
		{
			FullEntity entity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => entity = args as FullEntity;

			parser.Parse(new Line("Power", "D 00:29:52.7090966 PowerTaskList.DebugPrintPower() -     FULL_ENTITY - Updating [entityName=UNKNOWN ENTITY [cardType=INVALID] id=4 zone=DECK zonePos=0 cardId= player=1] CardID="));
			Assert.IsNotNull(entity);
			Assert.IsNotNull(entity.Data);
			Assert.AreEqual(4, entity.Data.Id);
			Assert.AreEqual(string.Empty, entity.Data.CardId);
			Assert.AreEqual(Zone.DECK, entity.Data.Zone);
		}

		[TestMethod]
		public void GameStateChange_Entity_PredictedCardId()
		{
			FullEntity entity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => entity = args as FullEntity;

			parser.Parse(new Line("Power", "D 00:31:50.9114028 PowerTaskList.DebugPrintPower() - BLOCK_START BlockType=POWER Entity=[entityName=King Togwaggle id=60 zone=PLAY zonePos=2 cardId=LOOT_541 player=2] EffectCardId= EffectIndex=0 Target=0 SubOption=-1"));
			parser.Parse(new Line("Power", "D 00:32:08.3649436 PowerTaskList.DebugPrintPower() -     FULL_ENTITY - Updating [entityName=UNKNOWN ENTITY [cardType=INVALID] id=61 zone=HAND zonePos=0 cardId= player=1] CardID="));
			Assert.IsNotNull(entity);
			Assert.AreEqual(CardIds.NonCollectible.Neutral.KingTogwaggle_KingsRansomToken, entity.Data.CardId);
		}

		[TestMethod]
		public void GameStateChange_TagChange()
		{
			TagChange tagChange = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => tagChange = args as TagChange;

			parser.Parse(new Line("Power", "D 00:31:50.8318211 PowerTaskList.DebugPrintPower() -     TAG_CHANGE Entity=Epix tag=RESOURCES_USED value=2"));
			Assert.IsNotNull(tagChange);
			Assert.IsNull(tagChange.EntityId);
			Assert.AreEqual("Epix", tagChange.EntityName);
			Assert.AreEqual(GameTag.RESOURCES_USED, tagChange.Tag);
			Assert.AreEqual(2, tagChange.Value);

			parser.Parse(new Line("Power", "D 00:31:50.8318211 PowerTaskList.DebugPrintPower() -     TAG_CHANGE Entity=2 tag=RESOURCES_USED value=2"));
			Assert.IsNotNull(tagChange);
			Assert.AreEqual(2, tagChange.EntityId);
			Assert.AreEqual(GameTag.RESOURCES_USED, tagChange.Tag);
			Assert.AreEqual(2, tagChange.Value);

			tagChange = null;
			parser.Parse(new Line("Power", "D 00:31:50.8568882 PowerTaskList.DebugPrintPower() -     TAG_CHANGE Entity=[entityName=Vilefin Inquisitor id=60 zone=HAND zonePos=4 cardId=OG_006 player=2] tag=ZONE value=PLAY"));
			Assert.IsNotNull(tagChange);
			Assert.AreEqual(60, tagChange.EntityId);
			Assert.AreEqual(GameTag.ZONE, tagChange.Tag);
			Assert.AreEqual((int)Zone.PLAY, tagChange.Value);
		}

		[TestMethod]
		public void GameStateChange_TagChange_CreationTag()
		{
			TagChange tagChange = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => tagChange = args as TagChange;

			parser.Parse(new Line("Power", "D 00:31:50.9189508 PowerTaskList.DebugPrintPower() -         tag=CONTROLLER value=2"));
			Assert.IsNotNull(tagChange);
			Assert.IsTrue(tagChange.CreationTag);
			Assert.AreEqual(GameTag.CONTROLLER, tagChange.Tag);
			Assert.AreEqual(2, tagChange.Value);
		}

		[TestMethod]
		public void GameStateChange_HideEntity()
		{
			HideEntity hideEntity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => hideEntity = args as HideEntity;

			parser.Parse(new Line("Power", "D 00:30:21.1930218 PowerTaskList.DebugPrintPower() -     HIDE_ENTITY - Entity=[entityName=Divine Favor id=70 zone=HAND zonePos=3 cardId=EX1_349 player=2] tag=ZONE value=DECK"));
			Assert.IsNotNull(hideEntity);
			Assert.AreEqual(70, hideEntity.EntityId);
		}

		[TestMethod]
		public void GameStateChange_ShowEntity()
		{
			ShowEntity showEntity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => showEntity = args as ShowEntity;

			parser.Parse(new Line("Power", "D 00:30:21.0673837 PowerTaskList.DebugPrintPower() -     SHOW_ENTITY - Updating Entity=[entityName=UNKNOWN ENTITY [cardType=INVALID] id=72 zone=DECK zonePos=0 cardId= player=2] CardID=UNG_015"));
			Assert.IsNotNull(showEntity);
			Assert.AreEqual(72, showEntity.EntityId);
			Assert.AreEqual("UNG_015", showEntity.CardId);
		}

		[TestMethod]
		public void GameStateChange_ChangeEntity()
		{
			ChangeEntity changeEntity = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.GameStateChange += args => changeEntity = args as ChangeEntity;

			parser.Parse(new Line("Power", "D 13:14:00.5727617 PowerTaskList.DebugPrintPower() -     CHANGE_ENTITY - Updating Entity=[entityName=Shifting Scroll id=28 zone=HAND zonePos=7 cardId=LOOT_104 player=1] CardID=UNG_948"));
			Assert.IsNotNull(changeEntity);
			Assert.AreEqual(28, changeEntity.EntityId);
			Assert.AreEqual("UNG_948", changeEntity.CardId);
		}

		[TestMethod]
		public void BlockStart()
		{
			IBlockData blockData = null;
			var parser = new PowerParser(new MockGameInfo());
			parser.BlockStart += args => blockData = args;

			parser.Parse(new Line("Power", "D 00:33:01.7253587 PowerTaskList.DebugPrintPower() - BLOCK_START BlockType=ATTACK Entity=[entityName=Vilefin Inquisitor id=60 zone=PLAY zonePos=2 cardId=OG_006 player=2] EffectCardId= EffectIndex=0 Target=[entityName=Jade Spirit id=40 zone=PLAY zonePos=2 cardId=CFM_715 player=1] SubOption=-1"));
			Assert.IsNotNull(blockData);
			Assert.AreEqual("OG_006", blockData.CardId);
			Assert.AreEqual(60, blockData.Id);
			Assert.IsNotNull(blockData.Target);
			Assert.AreEqual(40, blockData.Target.Id);
			Assert.AreEqual("CFM_715", blockData.Target.CardId);
			Assert.AreEqual(BlockType.ATTACK, blockData.Type);

			blockData = null;
			parser.Parse(new Line("Power", "D 00:33:04.6148508 PowerTaskList.DebugPrintPower() - BLOCK_START BlockType=TRIGGER Entity=Epix EffectCardId= EffectIndex=-1 Target=0 SubOption=-1 TriggerKeyword=0"));
			Assert.IsNotNull(blockData);
			Assert.IsNull(blockData.Target);
			Assert.AreEqual(BlockType.TRIGGER, blockData.Type);
		}

		[TestMethod]
		public void BlockStart_CanParseAllLines()
		{
			var validType = 0;
			var validTarget = 0;
			var data = TestData.Load("LogTests/TestData/data_1.json");
			var lines = File.ReadLines(data.LogFile).Where(x => x.Contains("PowerTaskList") && x.Contains("BLOCK_START")).ToList();
			var targets = lines.Where(x => x.Contains("Target=") && !x.Contains("Target=0") && !x.Contains("Target= ")).ToList();
			var parser = new PowerParser(new MockGameInfo());
			parser.BlockStart += args =>
			{
				if(args.Type != null)
					validType++;
				if(args.Target != null && args.Target.Id > 0)
					validTarget++;
			};
			foreach(var line in lines)
				parser.Parse(new Line("Power", line));
			Assert.AreEqual(197, lines.Count);
			Assert.AreEqual(197, validType);
			Assert.AreEqual(19, targets.Count);
			Assert.AreEqual(19, validTarget);
		}

		[TestMethod]
		public void BlockEnd()
		{
			var blockEnd = false;
			var parser = new PowerParser(new MockGameInfo());
			parser.BlockEnd += block => blockEnd = true;

			parser.Parse(new Line("Power", "D 00:30:21.2050280 PowerTaskList.DebugPrintPower() - BLOCK_END"));
			Assert.IsTrue(blockEnd);
		}

		[TestMethod]
		public void SetupComplete()
		{
			var count = 0;
			var parser = new PowerParser(new MockGameInfo());
			parser.SetupComplete += () => count++;

			parser.Parse(new Line("Power", "D 00:29:52.5947934 PowerTaskList.DebugDump() - ID=1 ParentID=0 PreviousID=0 TaskCount=88"));
			Assert.AreEqual(0, count);

			parser.Parse(new Line("Power", "D 00:29:55.3363610 PowerTaskList.DebugDump() - ID=2 ParentID=0 PreviousID=0 TaskCount=40"));
			Assert.AreEqual(1, count);

			parser.Parse(new Line("Power", "D 00:29:56.0075931 PowerTaskList.DebugDump() - ID=22 ParentID=0 PreviousID=0 TaskCount=1"));
			Assert.AreEqual(1, count);
		}
	}
}
