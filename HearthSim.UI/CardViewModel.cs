using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.UI.Annotations;
using HearthSim.UI.Themes;

namespace HearthSim.UI
{
	public class CardViewModel : INotifyPropertyChanged
	{
		public CardViewModel(Entity entity) : this(entity.Card)
		{
		}

		public CardViewModel(Card card)
		{
			Card = card;
			Count = card.Count;
			Cost = card.Data.Cost;
		}

		public Card Card { get; set; }
		public int Count { get; set; }
		public bool InHand { get; set; }
		public Rarity Rarity => Card.Data?.Rarity ?? Rarity.FREE;
		public string Id => Card.Id;
		public DrawingBrush Background => CardImageCache.Get(this);
		public ImageBrush Highlight => ThemeManager.CurrentTheme?.HighlightImage ?? new ImageBrush();
		public bool Created { get; set; }
		public bool Guessed { get; set; }
		public int Cost { get; set; }
		public SolidColorBrush TextColor { get; set; } = new SolidColorBrush(Colors.White);
		public string Name => Card.Data.Name;
		public bool Discarded { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void RefreshBackground() => OnPropertyChanged(nameof(Background));
		public void RefreshHighlight() => OnPropertyChanged(nameof(Highlight));

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
