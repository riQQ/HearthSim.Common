using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
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

		public string Name => _isLocalPlayer ? _gameState.MatchInfo.LocalPlayer.Name : _gameState.MatchInfo.OpposingPlayer.Name;

		public int PlayerId => PlayerEntity?.PlayerId ?? 0;

		public IEnumerable<Entity> Entities => _gameState.Entities.Values.Where(x => x.IsControlledBy(PlayerId));

		public IEnumerable<Entity> RevealedCards => _gameState.Entities.Values.Where(x => x.Info.OriginalController == PlayerId && x.IsPlayableCard && x.HasCardId && x.Info.OriginalZone != Zone.SETASIDE);

		public IEnumerable<Entity> InHand => Entities.Where(x => x.IsInHand);

		public IEnumerable<Entity> InDeck => Entities.Where(x => x.IsInDeck);

		public IEnumerable<Entity> InPlay => Entities.Where(x => x.IsInPlay);

		public IEnumerable<Entity> InGraveyard => Entities.Where(x => x.IsInGraveyard);

		public IEnumerable<Entity> InSecret => Entities.Where(x => x.IsInSecret && x.IsSecret);

		public IEnumerable<Entity> InQuest => Entities.Where(x => x.IsInSecret && x.IsQuest);

		public Deck Deck { get; set; }

		public IReadOnlyCollection<Card> GetRemainingCards()
		{
			if(Deck == null)
			{
				return RevealedCards.Where(x => x.Info.OriginalZone == Zone.HAND || x.Info.OriginalZone == Zone.DECK)
					.GroupBy(x => new {x.CardId, x.IsInDeck, x.IsCreated})
					.Select(x => new Card(x.Key.CardId, x.Key.IsInDeck ? -x.Count() : x.Count())).ToList();
			}
			var cards = Deck.Cards.Select(x => x.Clone()).ToList();
			foreach(var entity in RevealedCards.Where(x => !x.IsInDeck && !x.IsCreated))
			{
				var card = cards.FirstOrDefault(c => c.Id == entity.Info.OriginalCardId);
				if(card != null)
					card.Count--;
			}
			var inDeck = InDeck.Where(x => x.HasCardId).ToList();
			foreach(var entity in inDeck.Where(x => x.IsCreated).GroupBy(x => x.CardId))
				cards.Add(new Card(entity.Key, entity.Count()) {Created = true});
			foreach(var entity in inDeck.Where(x => x.Info.Stolen && !x.IsCreated).GroupBy(x => x.CardId))
			{
				var card = cards.FirstOrDefault(c => c.Id == entity.Key);
				if(card != null)
					card.Count++;
				else
					cards.Add(new Card(entity.Key, entity.Count()));
			}
			return cards;
		}
	}
}
