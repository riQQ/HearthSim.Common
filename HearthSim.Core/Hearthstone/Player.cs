using System.Collections.Generic;
using System.Linq;
using HearthSim.Core.Hearthstone.Entities;

namespace HearthSim.Core.Hearthstone
{
	public class Player
	{
		private readonly GameState _gameState;
		private readonly bool _isLocalPlayer;

		public Player(GameState gameState, bool isLocalPlayer)
		{
			_gameState = gameState;
			_isLocalPlayer = isLocalPlayer;
		}

		public PlayerEntity PlayerEntity => _isLocalPlayer ? _gameState.LocalPlayerEntity : _gameState.OpposingPlayerEntity;

		public int PlayerId => PlayerEntity?.PlayerId ?? 0;

		public IEnumerable<Entity> Entities => _gameState.Entities.Values.Where(x => x.IsControlledBy(PlayerId));

		public IEnumerable<Entity> RevealedCards => _gameState.Entities.Values.Where(x => (x.IsControlledBy(PlayerId) || x.Info.OriginalController == PlayerId) && !x.IsCreated && !x.Info.Stolen && x.IsPlayableCard && x.HasCardId);

		public IEnumerable<Entity> Hand => Entities.Where(x => x.IsInHand);

		public IEnumerable<Entity> Deck => Entities.Where(x => x.IsInDeck);

		public IEnumerable<Entity> Board => Entities.Where(x => x.IsInPlay);

		public IEnumerable<Entity> Graveyard => Entities.Where(x => x.IsInGraveyard);

		public IEnumerable<Entity> Secret => Entities.Where(x => x.IsInSecret && x.IsSecret);

		public IEnumerable<Entity> Quest => Entities.Where(x => x.IsInSecret && x.IsQuest);
	}
}
