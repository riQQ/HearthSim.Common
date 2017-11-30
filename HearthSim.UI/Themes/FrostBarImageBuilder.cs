using HearthSim.UI.Extensions;

namespace HearthSim.UI.Themes
{
	public class FrostBarImageBuilder : CardBarImageBuilder
	{
		public FrostBarImageBuilder(ThemeConfig config, CardViewModel card, string dir) : base(config, card, dir)
		{
		}

		protected override void AddCardImage() => AddCardImage(ImageRect.Move(-1, 0), false);

		protected override void AddCountText() => AddCountText(CountTextRect.Move(1, 0));

		protected override void AddLegendaryIcon() => AddLegendaryIcon(BoxRect.Move(-1,0));
	}
}
