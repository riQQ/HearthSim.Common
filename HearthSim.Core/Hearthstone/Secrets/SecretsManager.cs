using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.Events;

namespace HearthSim.Core.Hearthstone.Secrets
{
	public class SecretsManager
	{
		private readonly IGameState _gameState;
		private readonly List<Secret> _secrets;
		private readonly SecretSolver _secretSolver;

		public SecretsManager(IGameState gameState, GameStateEvents gameStateEvents)
		{
			_gameState = gameState;
			_secretSolver = new SecretSolver(gameState);
			_secrets = new List<Secret>();

			gameStateEvents.Attack += OnAttack;
			gameStateEvents.CardPlayed += OnCardPlayed;
			gameStateEvents.MinionDied += OnDeath;
			gameStateEvents.TagChange += OnTagChange;
			gameStateEvents.CardStolen += OnCardStolen;
		}

		private bool TryGetSecrets(Entity entity, out List<Secret> secrets)
		{
			secrets = null;
			return TryGetSecrets(entity.GetTag(GameTag.CONTROLLER), out secrets);
		}

		private bool TryGetSecrets(int playerId, out List<Secret> secrets)
		{
			var player = _gameState.PlayerEntities.Values.FirstOrDefault(x => x.PlayerId != playerId);
			if(player == null)
				secrets = new List<Secret>();
			else
				secrets = _secrets.Where(x => x.Entity.IsControlledBy(player.PlayerId)).ToList();
			return secrets.Any();
		}

		private void OnAttack(AttackGameEvent e)
		{
			if(!e.GameState.Entities.TryGetValue(e.Data.Attacker, out var attacker))
				return;
			if(!e.GameState.Entities.TryGetValue(e.Data.Defender, out var defender))
				return;
			if(!attacker.IsControlledBy(e.GameState.CurrentPlayerEntity?.PlayerId ?? 0))
				return;
			if(attacker.GetTag(GameTag.CONTROLLER) == defender.GetTag(GameTag.CONTROLLER))
				return;
			if(!TryGetSecrets(attacker, out var secrets))
				return;
			Resolve(_secretSolver.SolveAttack(attacker, defender), secrets);
		}

		private void OnCardPlayed(EntityGameEvent e)
		{
			if(!e.GameState.Entities.TryGetValue(e.Data, out var entity))
				return;
			if(!entity.IsControlledBy(e.GameState.CurrentPlayerEntity?.PlayerId ?? 0))
				return;
			if(!TryGetSecrets(entity, out var secrets))
				return;
			Resolve(_secretSolver.SolveCardPlayed(entity), secrets);
		}

		private async void OnDeath(EntityGameEvent e)
		{
			if(!e.GameState.Entities.TryGetValue(e.Data, out var entity))
				return;
			if(!entity.IsMinion)
				return;
			if(entity.IsControlledBy(e.GameState.CurrentPlayerEntity?.PlayerId ?? 0))
				return;
			if(!TryGetSecrets(e.GameState.CurrentPlayerEntity, out var secrets))
				return;
			var solved = _secretSolver.SolveMinionDeath(entity).ToList();
			if(entity.IsActiveDeathrattle)
				await _gameState.GameTime.Wait(500);
			solved.AddRange(_secretSolver.SolveMinionDeathDeathrattles(entity));
			Resolve(solved, secrets);
		}

		private void OnTagChange(TagChangeGameEvent e)
		{
			if(!e.Data.EntityId.HasValue || !e.GameState.Entities.TryGetValue(e.Data.EntityId.Value, out var entity))
				return;
			if(entity is GameEntity)
			{
				if(e.Data.Tag == GameTag.TURN && e.Data.Value > 0)
				{
					var currentPlayer = e.GameState.CurrentPlayerEntity;
					var s = _secrets.Where(x => x.Entity.IsControlledBy(currentPlayer.PlayerId)).ToList();
					Resolve(_secretSolver.SolveTurnStart(), s);
				}
			}
			if(entity.IsSecret && e.Data.Tag == GameTag.ZONE)
			{
				if(e.Data.PreviousValue != (int)Zone.SECRET && e.Data.Value == (int)Zone.SECRET)
					OnSecretPlayed(entity);
				if(e.Data.PreviousValue == (int)Zone.SECRET && e.Data.Value != (int)Zone.SECRET)
					OnSecretRemoved(entity);
			}
			if(!entity.IsControlledBy(e.GameState.CurrentPlayerEntity?.PlayerId ?? 0))
				return;
			if(!TryGetSecrets(entity, out var secrets))
				return;
			switch(e.Data.Tag)
			{
				case GameTag.CARDTYPE:
					if(e.Data.Value == (int) CardType.MINION)
						Resolve(_secretSolver.SolveMinionReveal(e), secrets);
					break;
				case GameTag.DAMAGE:
					if(e.Data.Value > 0)
						Resolve(_secretSolver.SolveDamageTaken(entity), secrets);
					break;
				case GameTag.CARD_TARGET:
					if(e.Data.Value > 0 && e.GameState.Entities.TryGetValue(e.Data.Value, out var target))
						Resolve(_secretSolver.SolveCardTarget(entity, target), secrets);
					break;
			}
		}

		private void OnCardStolen(EntityGameEvent e)
		{
			if(!e.GameState.Entities.TryGetValue(e.Data, out var entity))
				return;
			if(!entity.IsSecret)
				return;
			var secret = _secrets.FirstOrDefault(x => x.Entity == entity);
			if(secret == null)
				return;
			// TODO: Solve all but known
			// do this when secrets gets a cardid instead?
		}

		private async void Resolve(IEnumerable<string> solved, List<Secret> secrets)
		{
			await _gameState.GameTime.Wait(500);

			//this log statement fixes SecretTests.Secrets2 --- ???
			Console.WriteLine("[" + string.Join(", ", secrets.Select(x => x.Entity.Card.Name)) + "]");
			var triggered = secrets.Where(x => x.Entity.GetTag(GameTag.ZONE) != (int)Zone.SECRET && x.Entity.HasCardId).ToList();
			if(triggered.Any())
			{
			}
			var maxIndex = triggered.Any() ? triggered.Max(secrets.LastIndexOf) : secrets.Count;
			var interruptCombat = new[]
			{
				CardIds.Collectible.Hunter.ExplosiveTrap, // if attacker dies
				CardIds.Collectible.Hunter.FreezingTrap,
				CardIds.Collectible.Rogue.SuddenBetrayal, // new target (friendly, no more possible triggers)
				CardIds.Collectible.Mage.Vaporize,
				CardIds.Collectible.Paladin.NobleSacrifice, // new target
				CardIds.Collectible.Hunter.Misdirection, // new target
				CardIds.Collectible.Hunter.WanderingMonster, // new target
			};
			var interruptPlay = new[]
			{
				CardIds.Collectible.Hunter.Snipe,
				CardIds.Collectible.Mage.ExplosiveRunes
			};

			for(var i = 0; i < secrets.Count; i++)
			{
				if(triggered.Contains(secrets[i]))
					continue;
				if(i > maxIndex)
				{
					if(triggered.Any(t => interruptCombat.Contains(t.Entity.CardId)
						|| interruptPlay.Contains(t.Entity.CardId)))
					{
						secrets[i].Solve(triggered.Select(x => x.Entity.CardId));
					}
					else
					{
						secrets[i].Solve(solved);
					}
				}
				else
				{
					secrets[i].Solve(solved);
				}
			}
			// test case: -- EVERYTHING TRIGGERS
			// Secrets played: Wandering Monster, Freezing Trap, snake Trap
			// Freezing trap can't be wandering monster or anything triggered by minion target attack

			// test case:
			// Secrets played: Snake Trap, Explosive Trap, Freezing Trap
			// Snake Trap can't be any interrupt combat secret
			// Freezing Trap can still be anything but explosive
		}

		private void OnSecretPlayed(Entity entity)
		{
			_secrets.Add(new Secret(entity));
		}

		private void OnSecretRemoved(Entity entity)
		{
			var secret = _secrets.FirstOrDefault(x => x.Entity == entity);
			if(secret != null)
			{
				_secrets.Remove(secret);
				var secrets = _secrets.Where(x => x.Entity.IsControlledBy(entity.GetTag(GameTag.CONTROLLER)));
				foreach(var s in secrets)
					s.Solve(new[] {entity.CardId});
			}
		}

		public IReadOnlyCollection<Secret> GetSecrets(int playerId)
		{
			return _secrets.Where(x => x.Entity.IsControlledBy(playerId)).ToList().AsReadOnly();
		}
	}
}
