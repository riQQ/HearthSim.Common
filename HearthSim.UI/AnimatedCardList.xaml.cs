using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Util.Logging;
using HearthSim.UI.Annotations;
using HearthSim.UI.Themes;

namespace HearthSim.UI
{
	public partial class AnimatedCardList : INotifyPropertyChanged
	{
		//private readonly ObservableCollection<AnimatedCard> _animatedCards = new ObservableCollection<AnimatedCard>();
		public static readonly DependencyProperty AnimateProperty = DependencyProperty.Register("Animate", typeof(bool), typeof(AnimatedCardList), new PropertyMetadata(true));
		public static readonly DependencyProperty TransitionOpacityProperty = DependencyProperty.Register("TransitionOpacity", typeof(bool), typeof(AnimatedCardList), new PropertyMetadata(true));
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
			get { return _cardViewModels; }
			set
			{
				_cardViewModels = value; 
				OnPropertyChanged();
			}
		}

		public void Update(IEnumerable<Entity> entites)
		{
			Update(entites.Select(e => new CardViewModel(e)).ToList(), false);
		}

		public void Update(IEnumerable<Card> cards)
		{
			Update(cards.Select(c => new CardViewModel(c)).ToList(), false);
		}

		public void Update(List<CardViewModel> cards, bool reset)
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
					else if(existing.Count != card.Count || existing.InHand != card.InHand)
					{
						//var highlight = existing.Card.Count != card.Count;
						existing.Count = card.Count;
						existing.InHand = card.InHand;
						//TODO: trigger update animation
						existing.RefreshBackground();
					}
					else if(existing.Created != card.Created)
					{
						// does not trigger update nimation
						existing.RefreshBackground();
					}
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
						CardViewModels.Insert(CardViewModels.IndexOf(card), newCard);
						newCard.RefreshBackground();
						//TODO: trigger update animation
						newCards.Remove(newCard);
					}
				}
				foreach(var card in toRemove)
					RemoveCard(card.Item1, card.Item2);
				foreach(var card in newCards)
				{
					CardViewModels.Insert(cards.IndexOf(card), card);
					card.RefreshBackground();
					//TODO: trigger fadein
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private async void RemoveCard(CardViewModel card, bool fadeOut)
		{
			if(fadeOut)
			{
				//TODO: trigger fadeout
				//await card.FadeOut(card.Card.Count > 0);
			}
			CardViewModels.Remove(card);
		}

		private bool AreEqualForList(CardViewModel c1, CardViewModel c2)
		{
			return c1.Id == c2.Id && c1.Guessed == c2.Guessed && c1.Created == c2.Created
				   && (!ThemeManager.Config.IndicateDiscarded || c1.Discarded == c2.Discarded);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
