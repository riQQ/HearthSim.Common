using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.Events;
using static HearthDb.CardIds.Collectible;

namespace HearthSim.Core.Hearthstone.Secrets
{
	public class SecretSolver
	{
		private readonly IGameState _gameState;
		private Entity _awaitingSpellTarget;
		private List<Entity> _awaitingMinionReveal;

		// Fast combat secrets, even though they are implemented in HDT, currently don't have any effect.
		// The reason behind these is that any fast combat secret is safe to be considered "solved"
		// if a ATTACKER/DEFENDER based secret was triggerd.
		// So this is only relevany if multiple secrets are in play.
		// The current implementation in HDT "solves" all combat secrets for a second secret
		// even if the first one was triggered.
		private readonly List<string> _fastCombatSecrets = new List<string>
		{
			Hunter.FreezingTrap,
			Hunter.ExplosiveTrap,
			Hunter.Misdirection,
			Paladin.NobleSacrifice,
			Mage.Vaporize
		};

		public SecretSolver(IGameState gameState)
		{
			_gameState = gameState;
		}

		private bool TryGetPlayer(Entity entity, out Player player)
		{
			player = null;
			return TryGetPlayers(entity, out player, out var opponent);
		}

		private bool TryGetPlayers(Entity entity, out Player player, out Player opponent)
		{
			player = null;
			opponent = null;
			return TryGetPlayers(entity.GetTag(GameTag.CONTROLLER), out player, out opponent);
		}

		private bool TryGetPlayers(int playerId, out Player player, out Player opponent)
		{
			if(_gameState.LocalPlayer.PlayerId == playerId)
			{
				player = _gameState.LocalPlayer;
				opponent = _gameState.OpposingPlayer;
			}
			else if(_gameState.OpposingPlayer.PlayerId == playerId)
			{
				player = _gameState.OpposingPlayer;
				opponent = _gameState.LocalPlayer;
			}
			else
			{
				player = null;
				opponent = null;
			}
			return player != null && opponent != null;
		}

		internal IEnumerable<string> SolveAttack(Entity attacker, Entity defender)
		{
			if(!TryGetPlayers(attacker, out var player, out var opponent))
				yield break;
			var opponentMinions = opponent.InPlay.Count(x => x.IsMinion);
			var freeSpaceOnBoard = opponentMinions < 7;

			// Non fast combat?
			if(defender.IsHero)
			{
				yield return Mage.IceBarrier;
				if(freeSpaceOnBoard)
				{
					yield return Hunter.BearTrap;
					yield return Hunter.WanderingMonster;
				}
				if(attacker.IsMinion)
				{
					int ZonePos(Entity ent) => ent.GetTag(GameTag.ZONE_POSITION);
					bool IsAdjacent(Entity other) => Math.Abs(ZonePos(attacker) - ZonePos(other)) == 1;
					if(player.InPlay.Where(x => x.IsMinion && IsAdjacent(x)).Any(x => x.Health > 0 && !x.HasTag(GameTag.IMMUNE)))
						yield return Rogue.SuddenBetrayal;
				}
			}
			else if(freeSpaceOnBoard)
			{
				yield return Hunter.SnakeTrap;
				yield return Hunter.VenomstrikeTrap;
			}

			// Fast combat secrets
			if(freeSpaceOnBoard)
				yield return Paladin.NobleSacrifice;
			if(defender.IsHero)
			{
				yield return Hunter.ExplosiveTrap;
				if(opponentMinions > 0 || player.InPlay.Any(x => x.IsMinion))
					yield return Hunter.Misdirection;
				if(attacker.IsMinion)
					yield return Mage.Vaporize;
			}
			if(attacker.IsMinion)
				yield return Hunter.FreezingTrap;
		}

		internal IEnumerable<string> SolveMinionReveal(TagChangeGameEvent e)
		{
			if(_awaitingMinionReveal?.Any(x => x.Id == e.Data.EntityId) ?? false)
				yield return Hunter.HiddenCache;
		}

		internal IEnumerable<string> SolveCardPlayed(Entity entity)
		{
			if(!TryGetPlayers(entity, out var player, out var opponent))
				yield break;

			var freeSpaceOnBoard = opponent.InPlay.Count(x => x.IsMinion) < 7;
			var freeSpaceInHand = opponent.InHand.Count() < 10;

			if(entity.IsPlayableCard)
			{
				var numCardsPlayed = player.PlayerEntity?.GetTag(GameTag.NUM_CARDS_PLAYED_THIS_TURN);
				if(freeSpaceOnBoard && numCardsPlayed >= 3)
					yield return Hunter.RatTrap;
				if(freeSpaceInHand && numCardsPlayed >= 3)
					yield return Paladin.HiddenWisdom;
			}

			if(entity.IsSpell)
			{
				yield return Mage.Counterspell;
				if(freeSpaceInHand)
					yield return Mage.ManaBind;
				if(freeSpaceOnBoard)
				{
					yield return Hunter.CatTrick;
					_awaitingSpellTarget = entity;
				}
			}
			else if(entity.IsMinion)
			{
				yield return Hunter.Snipe;
				yield return Mage.ExplosiveRunes;
				yield return Mage.PotionOfPolymorph;
				yield return Paladin.Repentance;

				if(freeSpaceOnBoard)
					yield return Mage.MirrorEntity;
				if(freeSpaceInHand)
					yield return Mage.FrozenClone;
				if(player.InPlay.Count(x => x.IsMinion) > 3)
					yield return Paladin.SacredTrial;

				if(opponent.InHand.Any(x => x.IsMinion))
					yield return Hunter.HiddenCache;
				else if(opponent.InHand.Any(x => !x.HasCardId))
					_awaitingMinionReveal = opponent.InHand.ToList();
			} 
			else if(entity.IsHeroPower)
				yield return Hunter.DartTrap;
		}

		internal IEnumerable<string> SolveMinionDeath(Entity entity)
		{
			if(!TryGetPlayer(entity, out var opponent))
				yield break;
			var freeSpaceInHand = opponent.InHand.Count() < 10;
			if(freeSpaceInHand)
			{
				yield return Mage.Duplicate;
				yield return Paladin.GetawayKodo;
				yield return Rogue.CheatDeath;
			}

			var survivingMinions = opponent.InPlay.Any(x => x.IsMinion && x.Health > 0);
			if(survivingMinions)
				yield return Paladin.Avenge; // TODO: Test
		}

		internal IEnumerable<string> SolveMinionDeathDeathrattles(Entity entity)
		{
			if(!TryGetPlayer(entity, out var opponent))
				yield break;
			var freeSpaceOnBoard = opponent.InPlay.Count(x => x.IsMinion) < 7;
			if(freeSpaceOnBoard)
			{
				yield return Paladin.Redemption;
				yield return Mage.Effigy;
			}
		}

		internal IEnumerable<string> SolveDamageTaken(Entity entity)
		{
			if(!entity.IsHero)
				yield break;
			yield return Paladin.EyeForAnEye;
			yield return Rogue.Evasion;
		}

		internal IEnumerable<string> SolveTurnStart()
		{
			yield return Paladin.CompetitiveSpirit;
		}

		internal IEnumerable<string> SolveCardTarget(Entity entity, Entity target)
		{
			if(_awaitingSpellTarget == null || _awaitingSpellTarget.Id != entity.Id)
				yield break;
			_awaitingSpellTarget = null;
			if(target.IsMinion)
				yield return Mage.Spellbender;

		}
	}
}
