using System.Windows;
using HearthSim.UI.Extensions;

namespace HearthSim.UI.Themes
{
	public class ClassicBarImageBuilder : CardBarImageBuilder
	{
		private readonly Rect _fadeRect = new Rect(28, 0, 189, 34);
		private readonly Rect _imageRect = new Rect(108, 4, 108, 27);
		private readonly Rect _costRect = new Rect(6, 1, 25, 34);

		public ClassicBarImageBuilder(ThemeConfig config, CardViewModel card, string dir) : base(config, card, dir)
		{
			ImageOffset = FadeOffset = CreatedIconOffset = -19;
			CostFontSize = 18;
			CountFontSize = 16;
		}

		protected override void AddFadeOverlay() => AddFadeOverlay(_fadeRect, true);

		protected override void AddCardImage() => AddCardImage(_imageRect, true);

		protected override void AddCost() => AddCost(_costRect);

		protected override void AddCountText() => AddCountText(CountTextRect.Move(1, 0));
	}
}
