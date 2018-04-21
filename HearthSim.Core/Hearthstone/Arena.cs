using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using HearthSim.Core.Hearthstone.Enums;
using HearthSim.Core.Util.EventArgs;

namespace HearthSim.Core.Hearthstone
{
	public class Arena
	{
		public enum ArenaState
		{
			Unknown,
			Drafting,
			InProgress,
			Complete
		}

		public Deck Deck { get; private set; }
		public List<RewardData> Rewards { get; } = new List<RewardData>();
		public int Season { get; private set; }
		public int Wins { get; private set; }
		public int Losses { get; private set; }
		public int Slot { get; private set; }
		public ArenaState State { get; private set; }

		internal void Unload()
		{
			Deck = null;
			Slot = 0;
			Losses = 0;
			Wins = 0;
			Season = 0;
			Rewards.Clear();
			State = ArenaState.Unknown;
		}

		private void Update(ArenaInfo info)
		{
			Deck = new Deck(DeckType.Arena, info.Deck);
			Slot = info.CurrentSlot;
			Losses = info.Losses;
			Wins = info.Wins;
			Season = info.Season;
			Rewards.AddRange(info.Rewards);
		}

		public event Action<ArenaDeckComlpeteEventArgs> DraftComplete;
		public event Action<ArenaChoicesChangedEventArgs> DraftChoices;
		public event Action<ArenaCardPickedEventArgs> DraftPick;
		public event Action<ArenaRunCompleteEventArgs> RunComplete;

		internal void OnArenaRunComplete(ArenaRunCompleteEventArgs args)
		{
			Update(args.Info);
			State = ArenaState.Complete;
			RunComplete?.Invoke(args);
		}

		internal void OnArenaDraftComplete(ArenaDeckComlpeteEventArgs args)
		{
			Update(args.Info);
			State = ArenaState.InProgress;
			DraftComplete?.Invoke(args);
		}

		internal void OnArenaDraftChoices(ArenaChoicesChangedEventArgs args)
		{
			State = ArenaState.Drafting;
			DraftChoices?.Invoke(args);
		}

		internal void OnArenaDraftPick(ArenaCardPickedEventArgs args) => DraftPick?.Invoke(args);
	}
}
