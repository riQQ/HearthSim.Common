using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Card = HearthSim.Core.Hearthstone.Card;

namespace HearthSim.UI
{
	/// <summary>
	/// Interaction logic for AnimatedCard.xaml
	/// </summary>
	public partial class AnimatedCard
	{
		public AnimatedCard()
		{
			InitializeComponent();
		}

		public bool Animate
		{
			get => (bool)GetValue(AnimateProperty);
			set => SetValue(AnimateProperty, value);
		}

		public Card Card
		{
			get => (Card)GetValue(CardProperty);
			set => SetValue(CardProperty, value);
		}

		public bool TransitionOpacity
		{
			get => (bool)GetValue(TransitionOpacityProperty);
			set => SetValue(TransitionOpacityProperty, value);
		}

		public async Task FadeIn(bool fadeIn)
		{
			//Card.Update();
			if(fadeIn && Animate)
			{
				if(TransitionOpacity)
					await RunStoryBoard("StoryboardFadeIn");
				else
					await RunStoryBoard("StoryboardFadeInNoOpacity");
			}
		}

		public async Task FadeOut(bool highlight)
		{
			if(highlight && Animate)
				await RunStoryBoard("StoryboardUpdate");
			//Card.Update();
			if(Animate)
			{
				if(TransitionOpacity)
					await RunStoryBoard("StoryboardFadeOut");
				else
					await RunStoryBoard("StoryboardFadeOutNoOpacity");
			}
		}

		public async Task Update(bool highlight)
		{
			if(highlight && Animate)
				await RunStoryBoard("StoryboardUpdate");
			//Card.Update();
		}

		private readonly List<string> _runningStoryBoards = new List<string>();
		public static readonly DependencyProperty AnimateProperty = DependencyProperty.Register("Animate", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));
		public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof(object), typeof(AnimatedCard), new FrameworkPropertyMetadata(FOo));

		private static void FOo(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			Console.WriteLine(o);
		}

		public static readonly DependencyProperty TransitionOpacityProperty = DependencyProperty.Register("TransitionOpacity", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));

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
