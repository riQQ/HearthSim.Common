using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace HearthSim.UI
{
	/// <summary>
	/// Interaction logic for AnimatedCard.xaml
	/// </summary>
	public partial class AnimatedCard
	{
		public static readonly DependencyProperty AnimateProperty =
			DependencyProperty.Register("Animate", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));

		public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof(CardViewModel),
			typeof(AnimatedCard), new FrameworkPropertyMetadata(OnCardChanged));

		public static readonly DependencyProperty TransitionOpacityProperty =
			DependencyProperty.Register("TransitionOpacity", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));

		private readonly List<string> _runningStoryBoards = new List<string>();

		public AnimatedCard()
		{
			InitializeComponent();
		}

		public bool Animate
		{
			get => (bool)GetValue(AnimateProperty);
			set => SetValue(AnimateProperty, value);
		}

		public CardViewModel Card
		{
			get => (CardViewModel)GetValue(CardProperty);
			set => SetValue(CardProperty, value);
		}

		public bool TransitionOpacity
		{
			get => (bool)GetValue(TransitionOpacityProperty);
			set => SetValue(TransitionOpacityProperty, value);
		}

		public async void FadeIn()
		{
			if(!Animate)
				return;
			if(TransitionOpacity)
				await RunStoryBoard("StoryboardFadeIn");
			else
				await RunStoryBoard("StoryboardFadeInNoOpacity");
		}

		public async Task FadeOut(bool highlight)
		{
			if(highlight && Animate)
				await RunStoryBoard("StoryboardUpdate");
			Card.RefreshBackground();
			if(Animate)
			{
				if(TransitionOpacity)
					await RunStoryBoard("StoryboardFadeOut");
				else
					await RunStoryBoard("StoryboardFadeOutNoOpacity");
			}
		}

		public async void Update()
		{
			if(Animate)
				await RunStoryBoard("StoryboardUpdate");
			Card.RefreshBackground();
		}

		private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is AnimatedCard ac)
			{
				if(e.OldValue is CardViewModel oldCard)
					oldCard.Update -= ac.Update;
				var newCard = (CardViewModel)e.NewValue;
				newCard.Update += ac.Update;
				newCard.FadeOut = ac.FadeOut;
				if(newCard.FadeIn)
					ac.FadeIn();
			}
		}

		public async Task RunStoryBoard(string key)
		{
			if(_runningStoryBoards.Contains(key))
				return;
			_runningStoryBoards.Add(key);
			var sb = (Storyboard)FindResource(key);
			sb.Begin();
			await Task.Delay(sb.Duration.TimeSpan);
			_runningStoryBoards.Remove(key);
		}
	}
}
