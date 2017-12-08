using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Util;
using HearthSim.UI.Annotations;
using HearthSim.UI.Themes;

namespace HearthSim.UI
{
	public class CardViewModel : INotifyPropertyChanged, ISortableCard
	{
		public CardViewModel(Entity entity) : this(entity.Card)
		{
		}

		public CardViewModel(Card card)
		{
			Card = card;
		}

		public Card Card { get; set; }
		public int Count => Card?.Count ?? 0;
		public bool InHand { get; set; }
		public Rarity Rarity => Card.Data?.Rarity ?? Rarity.FREE;
		public string Id => Card?.Id;
		public DrawingBrush Background => CardImageCache.Get(this);
		public ImageBrush Highlight => ThemeManager.CurrentTheme?.HighlightImage ?? new ImageBrush();
		public bool Created { get; set; }
		public bool Guessed { get; set; }
		public int Cost => Card?.Data?.Cost ?? 0;
		public SolidColorBrush TextColor { get; set; } = new SolidColorBrush(Colors.White);
		public string Name => Card?.Data?.Name;
		public bool Discarded { get; set; }

		public Func<bool, Task> FadeOut { get; set; }
		public bool FadeIn { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action Update;
		public void RefreshBackground() => OnPropertyChanged(nameof(Background));
		public void RefreshHighlight() => OnPropertyChanged(nameof(Highlight));

		public void TriggerUpdate() => Update?.Invoke();
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

	}
}
