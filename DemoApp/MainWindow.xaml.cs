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
		private List<string> _localPlayerCards;
		private List<string> _localPlayerHand;
		private List<string> _localPlayerSecrets;
		private static Game _game;
		private List<string> _localPlayerGraveyard;
		private List<string> _localPlayerQuest;
		private string _questProgress;

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
			_game.State.OnModified += GameEvents.OnCardDrawn(e => Log.Debug($"DRAW Id={e.Id} CardId={e.CardId} Controller={e.GetTag(GameTag.CONTROLLER)}"));
			_game.State.OnModified += GameEvents.OnCardPlayed(e => Log.Debug($"PLAY Id={e.Id} CardId={e.CardId} Controller={e.GetTag(GameTag.CONTROLLER)}"));
			_game.State.OnModified += GameEvents.OnGameEnd(() => Log.Debug("GAME END"));
			_game.State.OnModified += (modifier, state) =>
			{
				var localPlayer = state.MatchInfo.LocalPlayer.Id;
				var player = state.PlayerEntities.FirstOrDefault(x => x.Value.PlayerId == localPlayer).Value;
				var mulliganState = player?.GetTag(GameTag.MULLIGAN_STATE);
				if(player != null && mulliganState != (int)Mulligan.DONE && modifier is TagChange t)
				{
					if(t.Tag == GameTag.ZONE)
					{
						if(t.Value == (int)Zone.HAND && mulliganState < (int)Mulligan.DEALING)
						{
							MulliganEntites[t.EntityId.Value] = false;
							OnPropertyChanged(nameof(LocalPlayerMulligan));
						}
						else if(t.Value == (int)Zone.DECK && t.PreviousValue == (int)Zone.HAND)
						{
							MulliganEntites[t.EntityId.Value] = true;
							OnPropertyChanged(nameof(LocalPlayerMulligan));
						}
					}
				}
			};

			_game.State.OnModified += UpdatePlayerCard;
		}

		private void UpdatePlayerCard(IGameStateModifier mod, GameState state)
		{
			var localPlayer = state.MatchInfo.LocalPlayer.Id;
			var entities = state.Entities.Select(x => x.Value);
			var playerEntities = entities.Where(x => x.GetTag(GameTag.CONTROLLER) == localPlayer && x.HasCardId).ToList();
			var playerRevealed = playerEntities.Where(x => x.HasCardId && x.IsPlayableCard && !x.IsCreated && !x.Info.Stolen);

			LocalPlayerCards = playerRevealed.Select(x => x.Card?.Name).ToList();
			LocalPlayerHand = playerEntities.Where(x => x.IsInHand).Select(x => x.Card?.Name).ToList();
			LocalPlayerGraveyard = playerEntities.Where(x => x.IsInGraveyard && x.IsPlayableCard).Select(x => x.Card?.Name).ToList();
			LocalPlayerSecrets = playerEntities.Where(x => x.IsInSecret && x.IsSecret).Select(x => x.Card?.Name).ToList();
			var quest = playerEntities.FirstOrDefault(x => x.IsInSecret && x.IsQuest);
			LocalPlayerQuest = new List<string> {quest?.Card?.Name};
			QuestProgress = quest != null ? $"{quest.GetTag(GameTag.QUEST_PROGRESS)}/{quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL)}" : "";
			OnPropertyChanged(nameof(MulliganEntites));
		}

		public Dictionary<int, bool> MulliganEntites { get; } = new Dictionary<int, bool>();

		public List<string> LocalPlayerMulligan => MulliganEntites.Select(x =>
			new {Entity = _game.State.Entities[x.Key], Mulliganed = x.Value}
			).Where(x => x.Entity.IsControlledBy(_game.State.MatchInfo.LocalPlayer.Id) && !x.Entity.IsCreated).Select(x => (x.Mulliganed? "[x]" : "") + x.Entity.Card?.Name)
			.ToList();

		public List<string> LocalPlayerCards
		{
			get => _localPlayerCards;
			set
			{
				_localPlayerCards = value; 
				OnPropertyChanged();
			}
		}

		public List<string> LocalPlayerHand
		{
			get => _localPlayerHand;
			set
			{
				_localPlayerHand = value; 
				OnPropertyChanged();
			}
		}

		public List<string> LocalPlayerSecrets
		{
			get => _localPlayerSecrets;
			set
			{
				_localPlayerSecrets = value; 
				OnPropertyChanged();
			}
		}

		public List<string> LocalPlayerGraveyard
		{
			get => _localPlayerGraveyard;
			set
			{
				_localPlayerGraveyard = value; 
				OnPropertyChanged();
			}
		}

		public List<string> LocalPlayerQuest
		{
			get => _localPlayerQuest;
			set
			{
				_localPlayerQuest = value; 
				OnPropertyChanged();
			}
		}

		public string QuestProgress
		{
			get => _questProgress;
			set
			{
				_questProgress = value;
				OnPropertyChanged();
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
