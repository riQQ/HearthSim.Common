using System.Windows;
using HearthSim.UI.Extensions;

namespace HearthSim.UI.Themes
{
	public class DarkBarImageBuilder : CardBarImageBuilder
	{
		private readonly Rect _fadeRect = new Rect(34, 0, 183, 34);
		public DarkBarImageBuilder(ThemeConfig config, CardViewModel card, string dir) : base(config, card, dir)
		{
		}

		protected override void AddFadeOverlay() => AddFadeOverlay(_fadeRect, true);

		protected override void AddCardImage() => AddCardImage(ImageRect, true);

		protected override void AddCountText() => AddCountText(CountTextRect.Move(2, 0));
	}
}
