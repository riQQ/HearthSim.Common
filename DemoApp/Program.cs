using System;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.GameStateModifiers;
using HearthSim.Core.LogReading;
using HearthSim.Core.LogParsing;
using HearthSim.Core.LogParsing.Parsers;
using HearthSim.Core.Util.Logging;

namespace DemoApp
{
	internal class Program
	{
		private static Game _game;
		private static void Main(string[] args)
		{
			Log.Initialize("D:/", "test");
			var watcher = new LogReader(
				"E:\\Program Files (x86)\\Hearthstone\\Logs",
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
			Console.ReadKey();
		}

		private static void PowerParser_GameStateChange(IGameStateModifier mod)
		{
			_game?.State.Apply(mod);
		}

		private static void PowerParser_CreateGame()
		{
			_game = new Game();
			//_game.State.OnModified += State_OnModified;
			_game.State.OnModified += GameEvents.OnCardDrawn(e => Log.Debug($"DRAW Id={e.Id} CardId={e.CardId} Controller={e.GetTag(GameTag.CONTROLLER)}"));
			_game.State.OnModified += GameEvents.OnCardPlayed(e => Log.Debug($"PLAY Id={e.Id} CardId={e.CardId} Controller={e.GetTag(GameTag.CONTROLLER)}"));
			_game.State.OnModified += GameEvents.OnGameEnd(() => Log.Debug("GAME END"));
		}

		private static void State_OnModified(IGameStateModifier mod, GameState state)
		{
			Log.Debug(mod.ToString());
		}
	}
}
