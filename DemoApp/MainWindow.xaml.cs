using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DemoApp.Annotations;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.LogReading;
using HearthSim.Core.Util;
using HearthSim.Core.Util.Logging;

namespace DemoApp
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		private static Game _game;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void PowerParser_GameStateChange(IGameStateModifier mod)
		{
			_game?.State.Apply(mod);
		}

		private void PowerParser_CreateGame()
		{
			_game = new Game();
			_game.State.OnModified += UpdateMulligan;
			_game.State.OnModified += UpdatePlayerCard;
		}

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

		private readonly Dictionary<int, bool> _mulliganEntites = new Dictionary<int, bool>();

		public IEnumerable<string> LocalPlayerMulligan => _game != null
			? _mulliganEntites.Select(x =>
					new {Entity = _game.State.Entities[x.Key], Mulliganed = x.Value}
				).Where(x => x.Entity.IsControlledBy(_game.State.MatchInfo.LocalPlayer.Id) && !x.Entity.IsCreated)
				.Select(x => (x.Mulliganed ? "[x]" : "") + x.Entity.Card?.Name)
			: null;

		public IEnumerable<string> LocalPlayerCards => _game?.State.LocalPlayer.RevealedCards.Select(x => x.Card?.Name);

		public IEnumerable<string> LocalPlayerHand => _game?.State.LocalPlayer.Hand.Select(x => x.Card?.Name);

		public IEnumerable<string> LocalPlayerSecrets => _game?.State.LocalPlayer.Secret.Select(x => x.Card?.Name);

		public IEnumerable<string> LocalPlayerGraveyard => _game?.State.LocalPlayer.Secret.Select(x => x.Card?.Name);

		public IEnumerable<string> LocalPlayerQuest => _game?.State.LocalPlayer.Quest.Select(x => x.Card?.Name);

		public string QuestProgress
		{
			get
			{
				var quest = _game?.State.LocalPlayer.Quest.FirstOrDefault();
				return quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			Log.Initialize("D:/", "test");

			var path = await HearthstoneProc.GetExecutablePath();

			var watcher = new LogReader(
				Path.Combine(path, "Logs"),
				LogWatcherConfigs.Power,
				LogWatcherConfigs.LoadingScreen,
				LogWatcherConfigs.FullScreenFx
			);


			var powerParser = new PowerParser();

			powerParser.CreateGame += PowerParser_CreateGame;
			powerParser.GameStateChange += PowerParser_GameStateChange;

			var parser = new LogParserManager();
			parser.RegisterParser(powerParser);

			watcher.NewLines += eventArgs => parser.Parse(eventArgs.Lines);
			watcher.Start();
		}
	}
}
