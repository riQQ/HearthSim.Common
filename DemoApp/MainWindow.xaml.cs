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
				Theme="Dark",
				RarityCardFrames = true,
				RarityCardGems = true
			});
		}

		private GameState Game => _core?.Game.CurrentGame;

		public IEnumerable<Card> LocalPlayerMulligan => Game != null
			? _mulliganEntites.Select(x =>
					new {Entity = Game.Entities[x.Key], Mulliganed = x.Value}
				).Where(x => x.Entity.IsControlledBy(Game.MatchInfo.LocalPlayer.Id) && !x.Entity.IsCreated && x.Entity.HasCardId)
				.Select(x => new Card(x.Entity.CardId, x.Mulliganed ? 0 : 1))
			: null;

		public IEnumerable<Card> OpposingPlayerMulligan => Game != null
			? _mulliganEntites.Select(x =>
					new {Entity = Game.Entities[x.Key], Mulliganed = x.Value}
				).Where(x => x.Entity.IsControlledBy(Game.MatchInfo.OpposingPlayer.Id) && !x.Entity.IsCreated && x.Entity.HasCardId)
				.Select(x => new Card(x.Entity.CardId, x.Mulliganed ? 0 : 1))
			: null;

		public IEnumerable<Card> LocalPlayerCards => Game?.LocalPlayer.GetRemainingCards();
		public IEnumerable<Card> OpposingPlayerCards => Game?.OpposingPlayer.GetRemainingCards();

		public IEnumerable<Entity> LocalPlayerHand => Game?.LocalPlayer.InHand;
		public IEnumerable<Entity> OpposingPlayerHand => Game?.OpposingPlayer.InHand;

		public IEnumerable<Entity> LocalPlayerSecrets => Game?.LocalPlayer.InSecret;
		public IEnumerable<Entity> OpposingPlayerSecrets => Game?.OpposingPlayer.InSecret;

		public IEnumerable<Entity> LocalPlayerGraveyard => Game?.LocalPlayer.InGraveyard;
		public IEnumerable<Entity> OpposingPlayerGraveyard => Game?.OpposingPlayer.InGraveyard;

		public IEnumerable<Entity> LocalPlayerQuest => Game?.LocalPlayer.InQuest;
		public IEnumerable<Entity> OpposingPlayerQuest => Game?.OpposingPlayer.InQuest;

		public string QuestProgress
		{
			get
			{
				var quest = Game?.LocalPlayer.InQuest.FirstOrDefault();
				return quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			}
		}

		public string OpposingQuestProgress
		{
			get
			{
				var quest = Game?.OpposingPlayer.InQuest.FirstOrDefault();
				return quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateMulligan(GameStateChangedEventArgs args)
		{
			if(args.Modifier is TagChange t)
			{
				void Foo(int playerId, int? mulligan)
				{
					if(mulligan.HasValue && mulligan != (int)Mulligan.DONE)
					{
						if(t.Tag == GameTag.ZONE && args.State.Entities[t.EntityId.Value].IsControlledBy(playerId))
						{
							if(t.Value == (int)Zone.HAND && mulligan < (int)Mulligan.DEALING)
								_mulliganEntites[t.EntityId.Value] = false;
							else if(t.Value == (int)Zone.DECK && t.PreviousValue == (int)Zone.HAND)
							{
								_mulliganEntites[t.EntityId.Value] = true;
								OnPropertyChanged(nameof(LocalPlayerMulligan));
								OnPropertyChanged(nameof(OpposingPlayerMulligan));
							}
						}
					}
				}

				var localPlayerMulligan = args.State.LocalPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE);
				var opposingPlayerMulligan = args.State.OpposingPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE);
				Foo(args.State.LocalPlayer.PlayerId, localPlayerMulligan);
				Foo(args.State.OpposingPlayer.PlayerId, opposingPlayerMulligan);
			}
		}

		private void UpdatePlayerCard(GameStateChangedEventArgs args)
		{
			if(!(args.Modifier is TagChange t))
				return;
			if(t.Tag == GameTag.ZONE || t.Tag == GameTag.STATE)
			{
				OnPropertyChanged(nameof(LocalPlayerCards));
				OnPropertyChanged(nameof(LocalPlayerHand));
				OnPropertyChanged(nameof(LocalPlayerSecrets));
				OnPropertyChanged(nameof(LocalPlayerGraveyard));
				OnPropertyChanged(nameof(LocalPlayerQuest));
				OnPropertyChanged(nameof(QuestProgress));

				OnPropertyChanged(nameof(OpposingPlayerCards));
				OnPropertyChanged(nameof(OpposingPlayerHand));
				OnPropertyChanged(nameof(OpposingPlayerSecrets));
				OnPropertyChanged(nameof(OpposingPlayerGraveyard));
				OnPropertyChanged(nameof(OpposingPlayerQuest));
				OnPropertyChanged(nameof(OpposingQuestProgress));
			}
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
			_core.Game.GameEnded += Game_GameEnded;
			_core.Start();
		}

		private void Game_GameEnded(GameEndEventArgs obj)
		{
			_mulliganEntites.Clear();
			OnPropertyChanged(nameof(LocalPlayerMulligan));
			OnPropertyChanged(nameof(OpposingPlayerMulligan));
		}
	}
}
