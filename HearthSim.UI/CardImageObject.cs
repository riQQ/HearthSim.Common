using System.Windows.Media;
using HearthSim.UI.Themes;

namespace HearthSim.UI
{
	internal class CardImageObject
	{
		public CardImageObject(DrawingBrush image, CardViewModel card) : this(card)
		{
			Image = image;
		}

		public CardImageObject(CardViewModel card)
		{
			Count = card.Count;
			Name = card.Name;
			Jousted = card.Guessed;
			ColoredFrame = ThemeManager.Config.RarityCardFrames;
			ColoredGem = ThemeManager.Config.RarityCardGems;
			Theme = ThemeManager.CurrentTheme?.Name;
			TextColorHash = card.TextColor.Color.GetHashCode();
			Created = card.Created;
		}

		public DrawingBrush Image { get; }
		public int Count { get; }
		public bool Jousted { get; }
		public bool ColoredFrame { get; }
		public bool ColoredGem { get; }
		public bool Created { get; }
		public string Theme { get; }
		public string Name { get; }
		public int TextColorHash { get; }

		public override bool Equals(object obj)
		{
			var cardObj = obj as CardImageObject;
			return cardObj != null && Equals(cardObj);
		}

		protected bool Equals(CardImageObject other)
			=> Count == other.Count && Jousted == other.Jousted && ColoredFrame == other.ColoredFrame
				&& ColoredGem == other.ColoredGem && Name == other.Name
				&& string.Equals(Theme, other.Theme) && TextColorHash == other.TextColorHash && Created == other.Created;

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Count;
				hashCode = (hashCode * 397) ^ Jousted.GetHashCode();
				hashCode = (hashCode * 397) ^ ColoredFrame.GetHashCode();
				hashCode = (hashCode * 397) ^ ColoredGem.GetHashCode();
				hashCode = (hashCode * 397) ^ (Theme?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ TextColorHash;
				hashCode = (hashCode * 397) ^ Created.GetHashCode();
				hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
				return hashCode;
			}
		}
	}
}
