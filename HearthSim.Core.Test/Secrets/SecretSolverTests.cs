using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.Events;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.Hearthstone.Secrets;
using HearthSim.Core.LogParsing.Parsers.Power;
using HearthSim.Core.Test.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static HearthDb.CardIds;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.Test.Secrets
{
	[TestClass]
	public class SecretSolverTests
	{
		private MatchInfo _matchInfo;
		private MockGameData _gameData;
		private GameState _game;
		private EntityHelper _helper;
		private PlayerEntity _player;
		private PlayerEntity _opponent;
		private SecretSolver _solver;
		private Entity _playerHero;
		private Entity _opponentHero;

		[TestInitialize]
		public void Initialize()
		{
			_matchInfo = new MatchInfo();
			_gameData = new MockGameData(_matchInfo);
			_game = new GameState(_gameData, new GameStateEvents());
			_helper = new EntityHelper(_game);
			_helper.CreateGame();
			_player = _helper.CreatePlayer();
			_opponent = _helper.CreatePlayer();
			_playerHero = _helper.Create(_player, Paladin.UtherLightbringer, Zone.PLAY);
			_opponentHero = _helper.Create(_opponent, Hunter.Rexxar, Zone.PLAY);

			_matchInfo.LocalPlayer = new MatchInfo.Player(_player.PlayerId, "", 0, 0, 0, 0, 0, 0, 0, null, null);
			_matchInfo.OpposingPlayer = new MatchInfo.Player(_opponent.PlayerId, "", 0, 0, 0, 0, 0, 0, 0, null, null);

			_solver = new SecretSolver(_game);
		}

		[TestMethod]
		public void Attack_MinionToMinion()
		{
			var attacker = _helper.Create(_player, Neutral.Wisp, Zone.PLAY);
			var defender = _helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			var secrets = _solver.SolveAttack(attacker, defender);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.SnakeTrap,
				Hunter.VenomstrikeTrap,
				Paladin.NobleSacrifice,
				Hunter.FreezingTrap
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 6; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(attacker, defender);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.FreezingTrap
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

		}

		[TestMethod]
		public void Attack_MinionToPlayer()
		{
			var attacker = _helper.Create(_player, Neutral.Wisp, Zone.PLAY);

			var secrets = _solver.SolveAttack(attacker, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Hunter.BearTrap,
				Hunter.WanderingMonster,
				Paladin.NobleSacrifice,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection,
				Mage.Vaporize,
				Hunter.FreezingTrap,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			_helper.Create(_player, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(attacker, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Hunter.BearTrap,
				Hunter.WanderingMonster,
				Rogue.SuddenBetrayal,
				Paladin.NobleSacrifice,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection,
				Mage.Vaporize,
				Hunter.FreezingTrap,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(attacker, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Rogue.SuddenBetrayal,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection,
				Mage.Vaporize,
				Hunter.FreezingTrap,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void Attack_PlayerToMinion()
		{
			var defender = _helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			var secrets = _solver.SolveAttack(_playerHero, defender);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.SnakeTrap,
				Hunter.VenomstrikeTrap,
				Paladin.NobleSacrifice,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(_playerHero, defender);
			Assert.IsTrue(secrets.SequenceEqual(new string[0]),
				string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void Attack_PlayerToPlayer()
		{
			var secrets = _solver.SolveAttack(_playerHero, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Hunter.BearTrap,
				Hunter.WanderingMonster,
				Paladin.NobleSacrifice,
				Hunter.ExplosiveTrap,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			_helper.Create(_player, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(_playerHero, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Hunter.BearTrap,
				Hunter.WanderingMonster,
				Paladin.NobleSacrifice,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);

			secrets = _solver.SolveAttack(_playerHero, _opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.IceBarrier,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void CardPlayed_Spell()
		{
			var target = _helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			var spell = _helper.Create(_player, Mage.Fireball, Zone.PLAY);
			var secrets = _solver.SolveCardPlayed(spell);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Counterspell,
				Mage.ManaBind,
				Hunter.CatTrick,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			secrets = _solver.SolveCardTarget(spell, target);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Spellbender,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			secrets = _solver.SolveCardTarget(spell, _opponentHero);
			Assert.AreEqual(0, secrets.Count());

			for(int i = 0; i < 10; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.HAND);
			secrets = _solver.SolveCardPlayed(spell);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Counterspell,
				Hunter.CatTrick,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			secrets = _solver.SolveCardPlayed(spell);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Counterspell,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			spell = _helper.Create(_player, Mage.Flamestrike, Zone.PLAY);
			_solver.SolveCardPlayed(spell);
			secrets = _solver.SolveCardTarget(spell, target);
			Assert.AreEqual(0, secrets.Count());
		}

		[TestMethod]
		public void CardPlayed_Minion()
		{
			var entity = _helper.Create(_player, Neutral.Wisp, Zone.PLAY);

			var secrets = _solver.SolveCardPlayed(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.Snipe,
				Mage.ExplosiveRunes,
				Mage.PotionOfPolymorph,
				Paladin.Repentance,
				Mage.MirrorEntity,
				Mage.FrozenClone,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			var inHand = _helper.Create(_opponent, Neutral.Wisp, Zone.HAND);
			inHand.CardId = null;
			inHand.SetTag(GameTag.CARDTYPE, 0);
			secrets = _solver.SolveCardPlayed(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.Snipe,
				Mage.ExplosiveRunes,
				Mage.PotionOfPolymorph,
				Paladin.Repentance,
				Mage.MirrorEntity,
				Mage.FrozenClone,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			inHand.CardId = Neutral.Wisp;
			inHand.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			secrets = _solver.SolveMinionReveal(new TagChangeGameEvent(new TagChange(new TagChangeData(GameTag.CARDTYPE, (int) CardType.MINION, false, inHand.Id, null)), _game));
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.HiddenCache,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 9; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.HAND);
			secrets = _solver.SolveCardPlayed(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.Snipe,
				Mage.ExplosiveRunes,
				Mage.PotionOfPolymorph,
				Paladin.Repentance,
				Mage.MirrorEntity,
				Hunter.HiddenCache
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
			
			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			secrets = _solver.SolveCardPlayed(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.Snipe,
				Mage.ExplosiveRunes,
				Mage.PotionOfPolymorph,
				Paladin.Repentance,
				Hunter.HiddenCache,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 3; i++)
				_helper.Create(_player, Neutral.Wisp, Zone.PLAY);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.Snipe,
				Mage.ExplosiveRunes,
				Mage.PotionOfPolymorph,
				Paladin.Repentance,
				Paladin.SacredTrial,
				Hunter.HiddenCache,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void CardPlayed_HeroPower()
		{
			var heroPower = _helper.Create(_player, NonCollectible.Mage.Fireblast, Zone.PLAY);
			var secrets = _solver.SolveCardPlayed(heroPower);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Hunter.DartTrap,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void MinionDeath()
		{
			var entity = _helper.Create(_opponent, Neutral.Wisp, Zone.GRAVEYARD);
			var secrets = _solver.SolveMinionDeath(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Duplicate,
				Paladin.GetawayKodo,
				Rogue.CheatDeath,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			var deadEntity = _helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			deadEntity.SetTag(GameTag.DAMAGE, 2);
			secrets = _solver.SolveMinionDeath(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Duplicate,
				Paladin.GetawayKodo,
				Rogue.CheatDeath
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			secrets = _solver.SolveMinionDeath(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Mage.Duplicate,
				Paladin.GetawayKodo,
				Rogue.CheatDeath,
				Paladin.Avenge
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}

		[TestMethod]
		public void DeathRattles()
		{
			var entity = _helper.Create(_opponent, Neutral.Wisp, Zone.GRAVEYARD);
			var secrets = _solver.SolveMinionDeathDeathrattles(entity);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Paladin.Redemption,
				Mage.Effigy
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			for(int i = 0; i < 7; i++)
				_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY);
			secrets = _solver.SolveMinionDeathDeathrattles(entity);
			Assert.AreEqual(0, secrets.Count());
		}

		[TestMethod]
		public void DamageTaken()
		{
			var secrets = _solver.SolveDamageTaken(_opponentHero);
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Paladin.EyeForAnEye,
				Rogue.Evasion
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));

			secrets = _solver.SolveDamageTaken(_helper.Create(_opponent, Neutral.Wisp, Zone.PLAY));
			Assert.AreEqual(0, secrets.Count());
		}

		[TestMethod]
		public void TurnStart()
		{
			var secrets = _solver.SolveTurnStart();
			Assert.IsTrue(secrets.SequenceEqual(new []
			{
				Paladin.CompetitiveSpirit,
			}), string.Join(", ", secrets.Select(x => Cards.All[x].Name)));
		}
	}
}
