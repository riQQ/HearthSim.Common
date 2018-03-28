using System;
using System.Collections.Generic;
using System.Linq;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class Collection
	{
		private readonly List<Deck> _decks;
		private List<CollectionCard> _cards;
		private List<CollectionCard> _favoriteHeroes;
		private List<int> _cardBacks;
		private int _dust;
		private int _gold;
		private int _favoriteCardBack;

		public Collection()
		{
			_cards = new List<CollectionCard>();
			_favoriteHeroes = new List<CollectionCard>();
			_cardBacks = new List<int>();
			_decks = new List<Deck>();
		}

		public bool IsLoaded { get; private set; }

		public IEnumerable<CollectionCard> Cards => _cards;
		public IEnumerable<Deck> Decks => _decks;
		public IEnumerable<CollectionCard> FavoriteHeroes => _favoriteHeroes;
		public IEnumerable<int> CardBacks => _cardBacks;

		public int FavoriteCardBack
		{
			get => _favoriteCardBack;
			private set => UpdateIfChanged(ref _favoriteCardBack, value, FavoriteCardBackChanged);
		}

		public int Dust
		{
			get => _dust;
			private set => UpdateIfChanged(ref _dust, value, DustChanged);
		}

		public int Gold
		{
			get => _gold;
			private set => UpdateIfChanged(ref _gold, value, GoldChanged);
		}

		private static void UpdateIfChanged<T>(ref T currentValue, T newValue,
			Action<ValueChangedEventArgs<T>> onChanged = null)
		{
			if(currentValue?.Equals(newValue) ?? false)
				return;
			var oldValue = currentValue;
			currentValue = newValue;
			onChanged?.Invoke(new ValueChangedEventArgs<T>(oldValue, newValue));
		}

		public event Action<CollectionChangedEventArgs<Deck>> DecksChanged;
		public event Action<CollectionChangedEventArgs<CollectionCard>> CardsChanged;
		public event Action<CollectionChangedEventArgs<CollectionCard>> FavoriteHeroesChanged;
		public event Action<CollectionChangedEventArgs<int>> CardBacksChanged;
		public event Action<ValueChangedEventArgs<int>> FavoriteCardBackChanged;
		public event Action<ValueChangedEventArgs<int>> DustChanged;
		public event Action<ValueChangedEventArgs<int>> GoldChanged;
		public event Action Changed;

		internal void OnDeckEdited(DeckEditedEventArgs args)
		{
			var index = _decks.FindIndex(x => x.DeckId == args.Deck.DeckId);
			if(index != -1)
			{
				_decks[index] = new Deck(args.Deck);
				DecksChanged?.Invoke(new CollectionChangedEventArgs<Deck>(Decks, edited: new [] {_decks[index]}));
			}
			else
			{
				var deck = new Deck(args.Deck);
				_decks.Add(deck);
				DecksChanged?.Invoke(new CollectionChangedEventArgs<Deck>(Decks, new [] {deck}));
			}
		}

		internal void OnDecksLoaded(ConstructedDeckFoundEventArgs args)
		{
			var decks = args.Decks.Select(x => new Deck(x));
			_decks.AddRange(decks);
			DecksChanged?.Invoke(new CollectionChangedEventArgs<Deck>(Decks));
		}

		internal void OnDeckDeleted(DeckDeletedEventArgs args)
		{
			var deck = Decks.FirstOrDefault(x => x.DeckId == args.DeckId);
			if(deck == null)
				return;
			_decks.Remove(deck);
			DecksChanged?.Invoke(new CollectionChangedEventArgs<Deck>(Decks, removed: new [] {deck}));
		}

		internal void Unload()
		{
			_cards.Clear();
			_decks.Clear();
			_favoriteHeroes.Clear();
			_cardBacks.Clear();
			FavoriteCardBack = 0;
			Gold = 0;
			Dust = 0;
			IsLoaded = false;
		}

		private static (List<CollectionCard> added, List<CollectionCard> removed)
			Diff(IEnumerable<CollectionCard> oldCards, ICollection<CollectionCard> newCards)
		{
			var added = new List<CollectionCard>();
			var removed = new List<CollectionCard>();
			foreach(var card in oldCards)
			{
				var newCard = newCards.FirstOrDefault(x => x.Id == card.Id);
				if(newCard != null)
				{
					var newNormal = newCard.Normal - card.Normal;
					var newGolden = newCard.Golden - card.Golden;
					if(newNormal > 0 || newGolden > 0)
						added.Add(new CollectionCard(card.Id, Math.Max(0, newNormal), Math.Max(0, newGolden)));
					if (newNormal < 0 || newGolden < 0)
						removed.Add(new CollectionCard(card.Id, Math.Max(0, -newNormal), Math.Max(0, -newGolden)));
					newCards.Remove(newCard);
				}
				else
					removed.Add(card);
			}
			added.AddRange(newCards);
			return (added, removed);
		}

		public void Update(IList<CollectionCard> cards, IList<int> cardBacks,
			IList<CollectionCard> favoriteHeroes, int favoriteCardBack, int dust, int gold)
		{
			var updatedCards = UpdateCards(cards);
			var updatedCardBacks = UpdateCardBacks(cardBacks);
			var updatedHeroes = UpdateFavoriteHeroes(favoriteHeroes);
			var updatedFavoriteCardBack = UpdateFavoriteCardBack(favoriteCardBack);
			var updatedDust = UpdateDust(dust);
			var updatedGold = UpdateGold(gold);
			if(updatedCards || updatedCardBacks || updatedHeroes || updatedFavoriteCardBack
				|| updatedDust || updatedGold)
			{
				Changed?.Invoke();
				IsLoaded = true;
			}
		}

		public bool UpdateFavoriteCardBack(int favoriteCardBack)
		{
			if(favoriteCardBack == FavoriteCardBack)
				return false;
			FavoriteCardBack = favoriteCardBack;
			return true;
		}

		public bool UpdateDust(int dust)
		{
			if(dust == Dust)
				return false;
			Dust = dust;
			return true;
		}

		public bool UpdateGold(int gold)
		{
			if(gold == Gold)
				return false;
			Gold = gold;
			return true;
		}

		private bool UpdateFavoriteHeroes(IList<CollectionCard> favoriteHeroes)
		{
			if(_favoriteHeroes.Count == 0)
			{
				_favoriteHeroes = favoriteHeroes.ToList();
				FavoriteHeroesChanged?.Invoke(new CollectionChangedEventArgs<CollectionCard>(_favoriteHeroes));
				return true;
			}
			var (added, removed) = Diff(_favoriteHeroes, favoriteHeroes.ToList());
			if(!added.Any() && !removed.Any())
				return false;
			_favoriteHeroes = favoriteHeroes.ToList();
			FavoriteHeroesChanged?.Invoke(new CollectionChangedEventArgs<CollectionCard>(_favoriteHeroes, added, removed));
			return true;
		}

		private bool UpdateCardBacks(IEnumerable<int> cardBacks)
		{
			if(_cardBacks.Count == 0)
			{
				_cardBacks = cardBacks.ToList();
				CardBacksChanged?.Invoke(new CollectionChangedEventArgs<int>(_cardBacks));
				return true;
			}
			var newCardBacks = cardBacks.ToList();
			var added = newCardBacks.Where(x => !_cardBacks.Contains(x)).ToList();
			var removed = _cardBacks.Where(x => !newCardBacks.Contains(x)).ToList();
			if(!added.Any() && !removed.Any())
				return false;
			_cardBacks = newCardBacks;
			CardBacksChanged?.Invoke(new CollectionChangedEventArgs<int>(_cardBacks, added, removed));
			return true;
		}

		private bool UpdateCards(IList<CollectionCard> cards)
		{
			if(_cards.Count == 0)
			{
				_cards = cards.ToList();
				CardsChanged?.Invoke(new CollectionChangedEventArgs<CollectionCard>(_cards));
				return true;
			}
			var (added, removed) = Diff(_cards, cards.ToList());
			if(!added.Any() && !removed.Any())
				return false;
			_cards = cards.ToList();
			CardsChanged?.Invoke(new CollectionChangedEventArgs<CollectionCard>(_cards, added, removed));
			return true;
		}
	}
}
