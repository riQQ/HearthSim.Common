using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Util.Logging;

namespace HearthSim.Core.Hearthstone
{
	public class Player
	{
		private readonly IGameState _gameState;
		private readonly bool _isLocalPlayer;
		private Deck _deck;

		internal event Action<ActivePlayerDeckChangedEventArgs> DeckChanged;

		public Player(IGameState gameState, bool isLocalPlayer)
		{
			_gameState = gameState;
			_isLocalPlayer = isLocalPlayer;
			PredictedCardIds = new HashSet<string>();
		}

		public PlayerEntity PlayerEntity => _isLocalPlayer ? _gameState.LocalPlayerEntity : _gameState.OpposingPlayerEntity;

		public string Name => _isLocalPlayer ? _gameState.MatchInfo?.LocalPlayer?.Name : _gameState.MatchInfo?.OpposingPlayer?.Name;

		public int PlayerId => PlayerEntity?.PlayerId ?? 0;

		public IEnumerable<Entity> Entities => _gameState.Entities.Values.Where(x => x.IsControlledBy(PlayerId));

		public IEnumerable<Entity> RevealedCards => _gameState.Entities.Values.Where(x => x.Info.OriginalController == PlayerId && x.IsPlayableCard && x.HasCardId && x.Info.OriginalZone != Zone.SETASIDE);

		public IEnumerable<Entity> InHand => Entities.Where(x => x.IsInHand);

		public IEnumerable<Entity> InDeck => Entities.Where(x => x.IsInDeck);

		public IEnumerable<Entity> InPlay => Entities.Where(x => x.IsInPlay);

		public IEnumerable<Entity> InGraveyard => Entities.Where(x => x.IsInGraveyard);

		public IEnumerable<Entity> InSecret => Entities.Where(x => x.IsInSecret && x.IsSecret);

		public IEnumerable<Entity> InQuest => Entities.Where(x => x.IsInSecret && x.IsQuest);

		public Entity CurrentHero => InPlay.FirstOrDefault(x => x.IsHero);

		public int SpellsPlayed { get; internal set; }

		public HashSet<string> PredictedCardIds { get; }

		public bool HasTheCoin => InHand.Any(x => x.CardId == CardIds.NonCollectible.Neutral.TheCoin);

		public Deck Deck
		{
			get => _deck;
			set
			{
				if(_deck != value)
				{
					_deck = value;
					Log.Debug($"{(_isLocalPlayer ? "Local Player" : "Opposing Player")} deck changed to {value}");
					DeckChanged?.Invoke(new ActivePlayerDeckChangedEventArgs(_isLocalPlayer, value));
				}
			}
		}

		public IReadOnlyCollection<Card> GetRemainingCards()
		{
			var cards = new List<Card>();
			if(Deck == null)
			{
				cards.AddRange(RevealedCards.Where(x => x.Info.OriginalZone == Zone.HAND || x.Info.OriginalZone == Zone.DECK)
					.GroupBy(x => new { x.CardId, x.IsInDeck, x.IsCreated })
					.Select(x => new Card(x.Key.CardId, x.Key.IsInDeck ? -x.Count() : x.Count()) { Created = x.Key.IsCreated })
					.ToList());
			}
			else
			{
				cards.AddRange(Deck.Cards.Select(x => x.Clone()).ToList());
				foreach(var entity in RevealedCards.Where(x => !(x.IsInDeck && x.IsControlledBy(PlayerId)) && !x.IsCreated))
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
			}
			foreach(var card in PredictedCardIds.GroupBy(x => x))
				cards.Add(new Card(card.Key, -card.Count()));
			return cards;
		}
	}
}
