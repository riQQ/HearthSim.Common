using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Util;
using HearthSim.UI.Annotations;
using HearthSim.UI.Themes;
using HearthSim.UI.Util;

namespace HearthSim.UI
{
	public class CardViewModel : INotifyPropertyChanged, ISortableCard, IDisposable
	{
		public CardViewModel(Card card, bool? created = null)
		{
			Card = card;
			Created = created ?? card.Created;
			CardImageCache.CardImageUpdated += OnImageCacheUpdated;
			ImageCache.FullCardUpdated += OnFullCardUpdated;
		}

		private void OnFullCardUpdated(string cardId)
		{
			if(cardId == Id)
				OnPropertyChanged(nameof(FullCardImage));
		}

		private void OnImageCacheUpdated(string cardId)
		{
			if(cardId == Id)
				OnPropertyChanged(nameof(TileImage));
		}

		public Card Card { get; set; }
		public int Count => Card?.Count ?? 0;
		public bool InHand { get; set; }
		public Rarity Rarity => Card.Data?.Rarity ?? Rarity.FREE;
		public string Id => Card?.Id;
		public DrawingBrush TileImage => CardImageCache.Get(this);
		public ImageBrush TileHighlight => ThemeManager.CurrentTheme?.HighlightImage ?? new ImageBrush();
		public ImageBrush FullCardImage => new ImageBrush(ImageCache.TryGetFullImage(Id));
		public bool Created { get; set; }
		public bool Guessed { get; set; }
		public int Cost => Card?.Data?.Cost ?? 0;
		public SolidColorBrush TextColor { get; set; } = new SolidColorBrush(Colors.White);
		public string Name => Card?.Name;
		public bool Discarded { get; set; }
		public int EntityId => Card?.EntityId ?? 0;
		public bool HideStats => Card?.Data?.Entity.GetTag(GameTag.HIDE_STATS) > 0;

		public Func<bool, Task> FadeOut { get; set; }
		public bool FadeIn { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<bool> Update;
		public void RefreshBackground() => OnPropertyChanged(nameof(TileImage));
		public void RefreshHighlight() => OnPropertyChanged(nameof(TileHighlight));

		public void TriggerUpdate(bool animate) => Update?.Invoke(animate);
		public async Task TriggerFadeOut(bool b)
		{
			if(FadeOut != null)
				await FadeOut.Invoke(b);
		}


		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Dispose()
		{
			CardImageCache.CardImageUpdated -= OnImageCacheUpdated;
		}
	}
}
