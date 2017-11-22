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
using HearthSim.Core.Util;
using HearthSim.Core.Util.Logging;

namespace DemoApp
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		private readonly Dictionary<int, bool> _mulliganEntites = new Dictionary<int, bool>();
		private Core _core;

		public MainWindow()
		{
			InitializeComponent();
		}

		private GameState Game => _core?.Game.CurrentGame;

		public IEnumerable<string> LocalPlayerMulligan => Game != null
			? _mulliganEntites.Select(x =>
					new {Entity = Game.Entities[x.Key], Mulliganed = x.Value}
				).Where(x => x.Entity.IsControlledBy(Game.MatchInfo.LocalPlayer.Id) && !x.Entity.IsCreated)
				.Select(x => (x.Mulliganed ? "[x]" : "") + x.Entity.Id + ": " +  x.Entity.Card?.Name)
			: null;

		public IEnumerable<Entity> LocalPlayerCards => Game?.LocalPlayer.RevealedCards;

		public IEnumerable<Entity> LocalPlayerHand => Game?.LocalPlayer.Hand;

		public IEnumerable<Entity> LocalPlayerSecrets => Game?.LocalPlayer.Secret;

		public IEnumerable<Entity> LocalPlayerGraveyard => Game?.LocalPlayer.Secret;

		public IEnumerable<Entity> LocalPlayerQuest => Game?.LocalPlayer.Quest;

		public string QuestProgress
		{
			get
			{
				var quest = Game?.LocalPlayer.Quest.FirstOrDefault();
				return quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateMulligan(IGameStateModifier modifier, GameState state)
		{
			var mulliganState = state.LocalPlayerEntity?.GetTag(GameTag.MULLIGAN_STATE);
			if(mulliganState.HasValue && mulliganState != (int)Mulligan.DONE && modifier is TagChange t)
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

		private void UpdatePlayerCard(IGameStateModifier mod, GameState state)
		{
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
			var path = await HearthstoneProc.GetExecutablePath();
			_core = new Core(path);
			_core.GameStateChanged += UpdateMulligan;
			_core.GameStateChanged += UpdatePlayerCard;
			_core.Start();
		}
	}
}
