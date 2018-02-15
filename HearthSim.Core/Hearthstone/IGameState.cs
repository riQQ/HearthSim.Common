using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public interface IGameState
	{
		DateTime CreatedAt { get; }
		int CurrentTurn { get; }
		Dictionary<int, Entity> Entities { get; }
		GameEntity GameEntity { get; set; }
		bool IsMulliganDone { get; }
		Entity LastCardPlayed { get; }
		Player LocalPlayer { get; }
		PlayerEntity LocalPlayerEntity { get; }
		MatchInfo MatchInfo { get; }
		Player OpposingPlayer { get; }
		PlayerEntity OpposingPlayerEntity { get; }
		Dictionary<int, PlayerEntity> PlayerEntities { get; }
		IReadOnlyCollection<string> PowerLog { get; }
		GameServerInfo ServerInfo { get; }
		bool SetupComplete { get; }
		GameTime GameTime { get; }
		PlayerEntity CurrentPlayer { get; }

		void AppendLog(LogEventArgs args);
	}
}
