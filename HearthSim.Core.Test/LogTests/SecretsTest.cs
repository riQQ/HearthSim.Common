using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.EventManagers;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Test.LogTests.TestHelpers;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.Test.LogTests
{
	[TestClass]
	public class SecretsTest
	{
		[TestMethod]
		public void Secrets1()
		{
			var data = TestData.Load("LogTests/TestData/secrets_1.json");
			var events = new Dictionary<string, object>();
			var gameData = new MockGameData(new MatchInfo()
			{
				LocalPlayer = data.LocalPlayer.ToMatchInfoPlayer(),
				OpposingPlayer = data.OpposingPlayer.ToMatchInfoPlayer()
			}, new GameServerInfo());
			var logInput = new MockLogInput();
			var game = new Game(gameData);
			var mgr = new GameEventManager(game, logInput, gameData);

			game.GameStateEvents.TagChange += args =>
			{
				if(args.Data.Tag == GameTag.TURN)
				{
					var secrets = game.CurrentGame.SecretsManager.GetSecrets(game.CurrentGame.LocalPlayer.PlayerId).ToList();
					switch(args.Data.Value)
					{
						case 5:
							events["t5"] = true;
							Assert.AreEqual(0, secrets.Count);
							break;
						case 6:
							events["t6"] = true;
							Assert.AreEqual(1, secrets.Count);
							Assert.AreEqual(12, secrets.Single().PossibleSecrets.Count);
							Assert.IsTrue(secrets.Single().RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Effigy,
								Mage.PotionOfPolymorph,
								Mage.Counterspell,
								Mage.IceBarrier,
								Mage.MirrorEntity,
								Mage.IceBlock,
								Mage.Vaporize,
								Mage.Duplicate,
								Mage.FrozenClone,
								Mage.ExplosiveRunes,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", secrets.Single().RemainingSecrets.Select(x => Cards.All[x].Name)));
							break;
						case 8:
						{
							events["t8"] = true;
							Assert.AreEqual(2, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Effigy,
								Mage.PotionOfPolymorph,
								Mage.MirrorEntity,
								Mage.IceBlock,
								Mage.Duplicate,
								Mage.FrozenClone,
								Mage.ExplosiveRunes,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, counterSpell.PossibleSecrets.Count);
							Assert.AreEqual(12, counterSpell.RemainingSecrets.Count);
							break;
						}
						case 10:
						{
							events["t10"] = true;
							Assert.AreEqual(3, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							var frozenClone = secrets.ElementAt(2);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Effigy,
								Mage.IceBlock,
								Mage.Duplicate,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(counterSpell.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Effigy,
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Duplicate,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, frozenClone.RemainingSecrets.Count);
							break;
						}
						case 12:
						{
							events["t12"] = true;
							Assert.AreEqual(2, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.IceBlock,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(counterSpell.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							break;
						}
						case 14:
						{
							events["t14"] = true;
							Assert.AreEqual(4, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							var potionOfPolymorph = secrets.ElementAt(2);
							var frozenClone = secrets.ElementAt(3);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.IceBlock,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(counterSpell.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, potionOfPolymorph.RemainingSecrets.Count);
							Assert.AreEqual(12, frozenClone.RemainingSecrets.Count);
							break;
						}
						case 16:
						{
							events["t16"] = true;
							Assert.AreEqual(4, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							var iceBarrier = secrets.ElementAt(2);
							var manaBind = secrets.ElementAt(3);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.IceBlock,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(counterSpell.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, iceBarrier.RemainingSecrets.Count);
							Assert.AreEqual(12, manaBind.RemainingSecrets.Count);
							break;
						}
						case 18:
						{
							events["t18"] = true;
							Assert.AreEqual(4, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var counterSpell = secrets.ElementAt(1);
							var manaBind = secrets.ElementAt(2);
							var iceBarrier = secrets.ElementAt(3);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.IceBlock,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(counterSpell.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.IsTrue(manaBind.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.IceBlock,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", manaBind.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, iceBarrier.RemainingSecrets.Count);
							break;
						}
						case 20:
						{
							events["t20"] = true;
							Assert.AreEqual(3, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var spellbender = secrets.ElementAt(1);
							var vaporize = secrets.ElementAt(2);
							Assert.IsTrue(iceBlock.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.IceBlock,
							}), string.Join("\n", iceBlock.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, spellbender.RemainingSecrets.Count);
							Assert.AreEqual(12, vaporize.RemainingSecrets.Count);
							break;
						}
						case 22:
						{
							events["t22"] = true;
							Assert.AreEqual(4, secrets.Count);
							var spellbender = secrets.ElementAt(0);
							var effigy = secrets.ElementAt(1);
							var duplicate = secrets.ElementAt(2);
							var manaBind = secrets.ElementAt(3);
							Assert.IsTrue(spellbender.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Counterspell,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", spellbender.RemainingSecrets.Select(x => Cards.All[x].Name)));
							Assert.AreEqual(12, effigy.RemainingSecrets.Count);
							Assert.AreEqual(12, duplicate.RemainingSecrets.Count);
							Assert.AreEqual(12, manaBind.RemainingSecrets.Count);
							break;
						}
					}
				}
			};

			logInput.Read(data.LogFile);
			foreach(var turn in new[] {"t5", "t6", "t8", "t10", "t12", "t14", "t16", "t18", "t20", "t22"})
				Assert.AreEqual(true, events[turn]);
		}

		[TestMethod]
		public void Secrets2()
		{
			var data = TestData.Load("LogTests/TestData/secrets_2.json");
			var events = new Dictionary<string, object>();
			var gameData = new MockGameData(new MatchInfo()
			{
				LocalPlayer = data.LocalPlayer.ToMatchInfoPlayer(),
				OpposingPlayer = data.OpposingPlayer.ToMatchInfoPlayer()
			}, new GameServerInfo());
			var logInput = new MockLogInput();
			var game = new Game(gameData);
			var mgr = new GameEventManager(game, logInput, gameData);

			game.GameStateEvents.TagChange += args =>
			{
				if(args.Data.Tag == GameTag.TURN)
				{
					var secrets = game.CurrentGame.SecretsManager.GetSecrets(game.CurrentGame.LocalPlayer.PlayerId).ToList();
					switch(args.Data.Value)
					{
						case 12:
						{
							events["t12"] = true;
							Assert.AreEqual(3, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var explosiveRune = secrets.ElementAt(1);
							var potionOfPolymorph = secrets.ElementAt(2);
							Assert.AreEqual(12, explosiveRune.RemainingSecrets.Count);
							Assert.AreEqual(12, potionOfPolymorph.RemainingSecrets.Count);
							break;
						}
						case 14:
						{
							// Minion played, Explosive Runes triggered because it was played first
							// can still be polymorph
							events["t14"] = true;
							Assert.AreEqual(2, secrets.Count);
							var iceBlock = secrets.ElementAt(0);
							var potionOfPolymorph = secrets.ElementAt(1);
							Assert.IsTrue(potionOfPolymorph.RemainingSecrets.SequenceEqual(new[]
							{
								Mage.Effigy,
								Mage.PotionOfPolymorph,
								Mage.Counterspell,
								Mage.IceBarrier,
								Mage.MirrorEntity,
								Mage.IceBlock,
								Mage.Vaporize,
								Mage.Duplicate,
								Mage.FrozenClone,
								Mage.Spellbender,
								Mage.ManaBind
							}), string.Join("\n", potionOfPolymorph.RemainingSecrets.Select(x => Cards.All[x].Name)));
							break;
						}
					}
				}
			};

			logInput.Read(data.LogFile);
			foreach(var turn in new[] {"t12", "t14"})
				Assert.AreEqual(true, events[turn]);
		}


		[TestMethod]
		public void Secrets3()
		{
			var data = TestData.Load("LogTests/TestData/secrets_3.json");
			var events = new Dictionary<string, object>();
			var gameData = new MockGameData(new MatchInfo()
			{
				LocalPlayer = data.LocalPlayer.ToMatchInfoPlayer(),
				OpposingPlayer = data.OpposingPlayer.ToMatchInfoPlayer()
			}, new GameServerInfo());
			var logInput = new MockLogInput();
			var game = new Game(gameData);
			var mgr = new GameEventManager(game, logInput, gameData);

			game.GameStateEvents.TagChange += args =>
			{
				if(args.Data.Tag == GameTag.TURN)
				{
					var secrets = game.CurrentGame.SecretsManager.GetSecrets(game.CurrentGame.LocalPlayer.PlayerId).ToList();
					switch(args.Data.Value)
					{
						case 12:
						{
							events["t12"] = true;
							//TODO
							break;
						}
					}
				}
			};

			logInput.Read(data.LogFile);
			foreach(var turn in new[] {"t12"})
				Assert.AreEqual(true, events[turn]);
		}
	}
}
