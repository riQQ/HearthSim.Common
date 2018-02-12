using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Util.Logging;
using HearthSim.UI.Annotations;
using HearthSim.UI.Themes;
using HearthSim.Util.Extensions;

namespace HearthSim.UI
{
	public partial class AnimatedCardList : INotifyPropertyChanged
	{
		public static readonly DependencyProperty AnimateProperty =
			DependencyProperty.Register("Animate", typeof(bool), typeof(AnimatedCardList), new PropertyMetadata(true));

		public static readonly DependencyProperty TransitionOpacityProperty =
			DependencyProperty.Register("TransitionOpacity", typeof(bool), typeof(AnimatedCardList), new PropertyMetadata(true));

		public static readonly DependencyProperty CardsProperty =
			DependencyProperty.Register("Cards", typeof(IEnumerable<Card>), typeof(AnimatedCardList),
				new FrameworkPropertyMetadata(default(IEnumerable<Card>), OnCardsChanged));

		public static readonly DependencyProperty EntitiesProperty =
			DependencyProperty.Register("Entities", typeof(IEnumerable<Entity>), typeof(AnimatedCardList),
				new FrameworkPropertyMetadata(default(IEnumerable<Entity>), FrameworkPropertyMetadataOptions.AffectsRender,
					OnEntitiesChanged));

		private readonly List<CardViewModel> _removing = new List<CardViewModel>();
		private ObservableCollection<CardViewModel> _cardViewModels;

		public AnimatedCardList()
		{
			InitializeComponent();
			CardViewModels = new ObservableCollection<CardViewModel>();
		}

		public bool Animate
		{
			get => (bool)GetValue(AnimateProperty);
			set => SetValue(AnimateProperty, value);
		}

		public bool TransitionOpacity
		{
			get => (bool)GetValue(TransitionOpacityProperty);
			set => SetValue(TransitionOpacityProperty, value);
		}

		public ObservableCollection<CardViewModel> CardViewModels
		{
			get => _cardViewModels;
			private set
			{
				_cardViewModels = value;
				var view = CollectionViewSource.GetDefaultView(_cardViewModels);
				view.SortDescriptions.Add(new SortDescription(nameof(CardViewModel.HideStats), ListSortDirection.Descending));
				view.SortDescriptions.Add(new SortDescription(nameof(CardViewModel.Cost), ListSortDirection.Ascending));
				view.SortDescriptions.Add(new SortDescription(nameof(CardViewModel.Name), ListSortDirection.Ascending));
				OnPropertyChanged();
			}
		}

		public IEnumerable<Card> Cards
		{
			get => (IEnumerable<Card>)GetValue(CardsProperty);
			set => SetValue(CardsProperty, value);
		}

		public IEnumerable<Entity> Entities
		{
			get => (IEnumerable<Entity>)GetValue(EntitiesProperty);
			set => SetValue(EntitiesProperty, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private static void OnEntitiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is AnimatedCardList cardList && cardList.Entities != null)
			{
				var cards = cardList.Entities.Where(x => x.HasCardId).GroupBy(x => new {x.CardId, x.IsCreated})
					.Select(g => new CardViewModel(new Card(g.Key.CardId, g.Count()), g.Key.IsCreated));
				cardList.Update(cards.ToList(), false);
			}
		}

		private static void OnCardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is AnimatedCardList cardList && cardList.Cards != null)
				cardList.Update(cardList.Cards.Select(x => new CardViewModel(x)).ToList(), false);
		}

		public void Update(List<CardViewModel> cards, bool reset)
		{
			UpdateAsync(cards, reset).Forget();
		}

		public async Task UpdateAsync(List<CardViewModel> cards, bool reset)
		{
			try
			{
				if(reset)
					CardViewModels.Clear();
				var newCards = new List<CardViewModel>();
				foreach(var card in cards)
				{
					var existing = CardViewModels.FirstOrDefault(x => AreEqualForList(x, card));
					if(existing == null)
						newCards.Add(card);
					else if(existing.Count != card.Count || existing.InHand != card.InHand || existing.Name != card.Name)
					{
						var highlight = existing.Card.Count != card.Count;
						existing.Card.Count = card.Count;
						existing.InHand = card.InHand;
						if(highlight)
							existing.TriggerUpdate();
						else
							existing.RefreshBackground();
					}
					else if(existing.Created != card.Created)
						existing.RefreshBackground();
				}
				var toUpdate = new List<CardViewModel>();
				foreach(var existing in CardViewModels)
				{
					if(!cards.Any(card => AreEqualForList(card, existing)))
						toUpdate.Add(existing);
				}
				var toRemove = new List<Tuple<CardViewModel, bool>>();
				foreach(var card in toUpdate)
				{
					var newCard = newCards.FirstOrDefault(x => x.Id == card.Card.Id);
					toRemove.Add(new Tuple<CardViewModel, bool>(card, newCard == null));
					if(newCard != null)
					{
						CardViewModels.Add(newCard);
						newCard.TriggerUpdate();
						newCards.Remove(newCard);
					}
				}
				await Task.WhenAll(toRemove.Select(card => RemoveCard(card.Item1, card.Item2)));
				foreach(var card in newCards)
				{
					CardViewModels.Add(card);
					card.RefreshBackground();
					card.FadeIn = true;
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private async Task RemoveCard(CardViewModel card, bool fadeOut)
		{
			if(_removing.Contains(card))
				return;
			_removing.Add(card);
			if(fadeOut)
				await card.TriggerFadeOut(card.Count > 0);
			CardViewModels.Remove(card);
			_removing.Remove(card);
		}

		private bool AreEqualForList(CardViewModel c1, CardViewModel c2)
		{
			return c1.Id == c2.Id && c1.Guessed == c2.Guessed && c1.Created == c2.Created && c1.Name == c2.Name
					&& c1.EntityId == c2.EntityId && (!ThemeManager.Config.IndicateDiscarded || c1.Discarded == c2.Discarded);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
