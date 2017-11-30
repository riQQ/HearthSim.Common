using System.Windows.Media;
using HearthDb.Enums;
using HearthSim.UI.Extensions;
using HearthSim.UI.Util;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace HearthSim.UI.Themes
{
	public class MinimalBarImageBuilder : CardBarImageBuilder
	{
		public MinimalBarImageBuilder(ThemeConfig config, CardViewModel card, string dir) : base(config, card, dir)
		{
			CreatedIconOffset = -15;
		}

		protected override void AddCardImage()
		{
			var bmp = ImageCache.GetCardBitmap(Card.Card);
			if(bmp == null)
				return;
			var img = new GaussianBlur(bmp).Process(2);
			DrawingGroup.Children.Add(new ImageDrawing(img.ToImageSource(), FrameRect));
		}

		protected override void AddCountBox()
		{
		}

		protected override SolidColorBrush CountTextBrush
		{
			get
			{
				switch(Card.Rarity)
				{
					case Rarity.RARE:
						return new SolidColorBrush(Color.FromRgb(49, 134, 222));
					case Rarity.EPIC:
						return new SolidColorBrush(Color.FromRgb(173, 113, 247));
					case Rarity.LEGENDARY:
						return new SolidColorBrush(Color.FromRgb(255, 154, 16));
					default:
						return Brushes.White;
				}
			}
		}
	}
}
