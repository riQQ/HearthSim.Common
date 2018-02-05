using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Test.MockData
{
	public class MockGameState : IGameState
	{
		public DateTime CreatedAt { get; set; }
		public int CurrentEntity { get; set; }
		public int CurrentTurn { get; set; }
		public GameEntity GameEntity { get; set; }
		public Entity LastCardPlayed { get; set; }
		public Player LocalPlayer { get; set; }
		public PlayerEntity LocalPlayerEntity { get; set; }
		public MatchInfo MatchInfo { get; set; }
		public Player OpposingPlayer { get; set; }
		public PlayerEntity OpposingPlayerEntity { get; set; }
		public Dictionary<int, PlayerEntity> PlayerEntities { get; set; }
		public IReadOnlyCollection<string> PowerLog { get; set; }
		public GameServerInfo ServerInfo { get; set; }
		public void AppendLog(LogEventArgs args)
		{
		}

		public Dictionary<int, Entity> Entities { get; set; } = new Dictionary<int, Entity>();
		public bool IsMulliganDone { get; set; }
		public bool SetupComplete { get; set; }
	}
}
