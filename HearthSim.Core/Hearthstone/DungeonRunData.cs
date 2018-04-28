using System.Linq;
using HearthDb.Enums;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.Util.Watchers;

namespace HearthSim.Core.Hearthstone
{
	public class DungeonRunData : DungeonRunWatcher.IDungeonRunDataProvider
	{
		private readonly Game _game;
		private readonly IGameDataProvider _gameData;

		public DungeonRunData(Game game, IGameDataProvider gameData)
		{
			_game = game;
			_gameData = gameData;
		}

		public bool InAiMatch => _game.CurrentMode == Mode.GAMEPLAY
								&& _game.CurrentGame?.MatchInfo?.GameType == (int)GameType.GT_VS_AI;

		public bool InAdventureScreen => _game.CurrentMode == Mode.ADVENTURE;
		public string OpponentHeroId => _game.CurrentGame?.OpposingPlayer.InPlay.FirstOrDefault(x => x.IsHero)?.CardId;

		public CardClass LocalPlayerClass =>
			_game.CurrentGame?.LocalPlayer.InPlay.FirstOrDefault(x => x.IsHero)?.Card?.Data?.Class ?? CardClass.INVALID;

		public DungeonInfo[] GetDungeonInfo() => _gameData.GetDungeonInfo();
	}
}
