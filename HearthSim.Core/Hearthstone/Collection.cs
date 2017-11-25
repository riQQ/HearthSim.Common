using System;
using System.Collections.Generic;
using System.Linq;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class Collection
	{
		private readonly List<Deck> _decks;
		private List<CollectionCard> _collectionCards;

		public Collection()
		{
			_collectionCards = new List<CollectionCard>();
			_decks = new List<Deck>();
		}

		public IReadOnlyCollection<CollectionCard> CollectionCards => _collectionCards.AsReadOnly();
		public IReadOnlyCollection<Deck> Decks => _decks.AsReadOnly();

		public event Action<CollectionDecksLoadedEventArgs> DecksLoaded;
		public event Action<CollectionDeckChangedEventArgs> DeckAdded;
		public event Action<CollectionDeckChangedEventArgs> DeckRemoved;
		public event Action<CollectionDeckChangedEventArgs> DeckEdited;

		public event Action<CollectionCardsLoadedEventArgs> CardsLoaded;
		public event Action<CollectionCardChangedEventArgs> CardsAdded;
		public event Action<CollectionCardChangedEventArgs> CardsRemoved;


		internal void OnDeckEdited(DeckEditedEventArgs args)
		{
			var index = _decks.FindIndex(x => x.DeckId == args.Deck.DeckId);
			if(index != -1)
			{
				_decks[index] = new Deck(args.Deck);
				DeckEdited?.Invoke(new CollectionDeckChangedEventArgs(_decks[index]));
			}
			else
			{
				var deck = new Deck(args.Deck);
				_decks.Add(deck);
				DeckAdded?.Invoke(new CollectionDeckChangedEventArgs(deck));
			}
		}

		internal void OnDecksLoaded(ConstructedDeckFoundEventArgs args)
		{
			DecksLoaded?.Invoke(new CollectionDecksLoadedEventArgs(args.Decks.Select(x => new Deck(x))));
		}

		internal void OnDeckDeleted(DeckDeletedEventArgs args)
		{
			var deck = Decks.FirstOrDefault(x => x.DeckId == args.DeckId);
			if(deck == null)
				return;
			_decks.Remove(deck);
			DeckRemoved?.Invoke(new CollectionDeckChangedEventArgs(deck));
		}

		internal void UpdateCards(IList<CollectionCard> cards)
		{
			if(_collectionCards.Count == 0)
			{
				_collectionCards = cards.ToList();
				CardsLoaded?.Invoke(new CollectionCardsLoadedEventArgs(CollectionCards));
			}
			else
			{
				var newCards = cards.ToList();
				var addedCards = new List<CollectionCard>();
				var removedCards = new List<CollectionCard>();
				foreach(var card in _collectionCards)
				{
					var newCard = newCards.FirstOrDefault(x => x.Id == card.Id);
					if(newCard != null)
					{
						var newNormal = newCard.Normal - card.Normal;
						var newGolden = newCard.Golden - card.Golden;
						if(newNormal > 0 || newGolden > 0)
							addedCards.Add(new CollectionCard(card.Id, Math.Max(0, newNormal), Math.Max(0, newGolden)));
						else if (newNormal < 0 || newGolden < 0)
							removedCards.Add(new CollectionCard(card.Id, Math.Max(0, Math.Abs(newNormal)), Math.Max(0, Math.Abs(newGolden))));
						newCards.Remove(newCard);
					}
					else
						removedCards.Add(card);
				}
				addedCards.AddRange(newCards);
				_collectionCards = cards.ToList();
				if(addedCards.Count > 0)
					CardsAdded?.Invoke(new CollectionCardChangedEventArgs(addedCards));
				if(removedCards.Count > 0)
					CardsRemoved?.Invoke(new CollectionCardChangedEventArgs(removedCards));
			}
		}
	}
}
