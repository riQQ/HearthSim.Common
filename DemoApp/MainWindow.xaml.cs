using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DemoApp.Annotations;
using HearthDb.Enums;
using HearthSim.Core;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.HSReplay;
using HearthSim.Core.Util;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Exceptions;
using HearthSim.Core.Util.Logging;
using HearthSim.UI.Themes;

namespace DemoApp
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		private readonly Dictionary<int, bool> _mulliganEntites = new Dictionary<int, bool>();
		private Core _core;

		public MainWindow()
		{
			InitializeComponent();
			ThemeManager.Load(new ThemeConfig
			{
				Theme="Dark"
			});
		}

		private GameState Game => _core?.Game.CurrentGame;

		public IEnumerable<string> LocalPlayerMulligan => Game != null
			? _mulliganEntites.Select(x =>
					new {Entity = Game.Entities[x.Key], Mulliganed = x.Value}
				).Where(x => x.Entity.IsControlledBy(Game.MatchInfo.LocalPlayer.Id) && !x.Entity.IsCreated)
				.Select(x => (x.Mulliganed ? "[x]" : "") + x.Entity.Id + ": " +  x.Entity.Card?.Data?.Name)
			: null;

		public IEnumerable<Card> LocalPlayerCards => Game?.LocalPlayer.GetRemainingCards();

		public IEnumerable<Entity> LocalPlayerHand => Game?.LocalPlayer.InHand;

		public IEnumerable<Entity> LocalPlayerSecrets => Game?.LocalPlayer.InSecret;

		public IEnumerable<Entity> LocalPlayerGraveyard => Game?.LocalPlayer.InSecret;

		public IEnumerable<Entity> LocalPlayerQuest => Game?.LocalPlayer.InQuest;

		public string QuestProgress
		{
			get
			{
				var quest = Game?.LocalPlayer.InQuest.FirstOrDefault();
				return quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateMulligan(GameStateChangedEventArgs args)
		{
			var mulliganState = args.State.LocalPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE);
			if(mulliganState.HasValue && mulliganState != (int)Mulligan.DONE && args.Modifier is TagChange t)
			{
				if(t.Tag == GameTag.ZONE)
				{
					if(t.Value == (int)Zone.HAND && mulliganState < (int)Mulligan.DEALING)
						_mulliganEntites[t.EntityId.Value] = false;
					else if(t.Value == (int)Zone.DECK && t.PreviousValue == (int)Zone.HAND)
					{
						_mulliganEntites[t.EntityId.Value] = true;
						OnPropertyChanged(nameof(LocalPlayerMulligan));
					}
				}
			}
		}

		private void UpdatePlayerCard(GameStateChangedEventArgs gameStateChangedEventArgs)
		{
			PlayerCards1.Update(LocalPlayerCards);
			OnPropertyChanged(nameof(LocalPlayerCards));
			OnPropertyChanged(nameof(LocalPlayerHand));
			OnPropertyChanged(nameof(LocalPlayerSecrets));
			OnPropertyChanged(nameof(LocalPlayerGraveyard));
			OnPropertyChanged(nameof(LocalPlayerQuest));
			OnPropertyChanged(nameof(QuestProgress));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			Log.Initialize("D:/", "test");
			string path;
			try
			{
				path = await HearthstoneProc.GetExecutablePath();
			}
			catch(HearthstoneNotFoundException ex)
			{
				Log.Error(ex);
				return;
			}
			var config = new HSReplayNetConfig(
				"C:\\Users\\Alex\\AppData\\Roaming\\HSC_TEST",
				"089b2bc6-3c26-4aab-adbe-bcfd5bb48671",
				"jIpNwuUWLFI6S3oeQkO3xlW6UCnfogw1IpAbFXqq",
				"OSC_TEST",
				new[] {BnetGameType.BGT_VS_AI},
				true,
				28403515
			);
			_core = new Core(path, config);
			_core.Game.GameStateChanged += UpdateMulligan;
			_core.Game.GameStateChanged += UpdatePlayerCard;
			_core.Start();
		}
	}
}
