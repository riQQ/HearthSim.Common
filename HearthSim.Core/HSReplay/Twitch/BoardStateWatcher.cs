using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HearthSim.Core.Hearthstone.Entities;
using HearthSim.Core.HSReplay.Twitch.Data;

namespace HearthSim.Core.HSReplay.Twitch
{
	internal class BoardStateWatcher
	{
		private const int UpdateDelay = 1000;
		private const int RepeatDelay = 10000;
		private bool _update;
		private bool _running;
		private BoardState _currentBoardState;
		private DateTime _currentBoardStateTime = DateTime.MinValue;
		public event Action<BoardState> OnNewBoardState;

		public void Stop()
		{
			_update = false;
			_currentBoardState = null;
		}

		public async void Start(IGameState game)
		{
			if(_running)
				return;
			_running = true;
			_update = true;
			while(_update)
			{
				var boardState = GetBoardState(game);
				var delta = (DateTime.Now - _currentBoardStateTime).TotalMilliseconds;
				var forceInvoke = delta > RepeatDelay && boardState != null && _currentBoardState != null;
				if(forceInvoke || (!boardState?.Equals(_currentBoardState) ?? false))
				{
					OnNewBoardState?.Invoke(boardState);
					_currentBoardState = boardState;
					_currentBoardStateTime = DateTime.Now;
				}
				await Task.Delay(UpdateDelay);
			}
			_running = false;
		}

		private BoardState GetBoardState(IGameState game)
		{
			if(game.LocalPlayerEntity == null || game.OpposingPlayerEntity == null)
				return null;

			int ZonePosition(Entity e) => e.GetTag(GameTag.ZONE_POSITION);
			int DbfId(Entity e) => e?.Card?.Data?.DbfId ?? 0;
			int[] SortedDbfIds(IEnumerable<Entity> entities) => entities.OrderBy(ZonePosition).Select(DbfId).ToArray();
			int HeroId(Entity playerEntity) => playerEntity.GetTag(GameTag.HERO_ENTITY);
			int WeaponId(Entity playerEntity) => playerEntity.GetTag(GameTag.WEAPON);
			Entity Find(Player p, int entityId) => p.Entities.FirstOrDefault(x => x.Id == entityId);
			Entity FindHeroPower(Player p) => p.Entities.FirstOrDefault(x => x.IsHeroPower && x.IsInPlay);
			BoardStateQuest Quest(Entity questEntity)
			{
				if(questEntity == null)
					return null;
				return new BoardStateQuest
				{
					DbfId = questEntity.Card?.Data?.DbfId ?? 0,
					Progress = questEntity.GetTag(GameTag.QUEST_PROGRESS),
					Total = questEntity.GetTag(GameTag.QUEST_PROGRESS_TOTAL)
				};
			}

			var player = game.LocalPlayer;
			var opponent = game.OpposingPlayer;

			var deck = player.Deck;
			var fullDeckList = player.Deck?.Cards.ToDictionary(x => x.Data?.DbfId, x => x.Count);
			int FullCount(int dbfId) => fullDeckList == null ? 0 : fullDeckList.TryGetValue(dbfId, out var count) ? count : 0;

			var playerCardsDict = new List<int[]>();
			var hero = 0;
			if(deck != null)
			{
				foreach(var card in player.GetRemainingCards())
				{
					if(card.Data?.DbfId > 0)
						playerCardsDict.Add(new []{card.Data.DbfId, card.Count, FullCount(card.Data.DbfId)});
				}
				hero = HeroCardId.TryGetValue(deck.Class, out var cardId) ? Cards.All[cardId].DbfId : 0;
			}


			return new BoardState
			{
				Player = new BoardStatePlayer
				{
					Board = SortedDbfIds(player.InPlay.Where(x => x.IsMinion)),
					Deck = new BoardStateDeck
					{
						Cards =  playerCardsDict,
						Name = deck?.Name,
						Format = deck?.Format ?? FormatType.FT_WILD,
						Hero = hero,
						//Wins = games?.Count(g => g.Result == GameResult.Win) ?? 0,
						//Losses = games?.Count(g => g.Result == GameResult.Loss) ?? 0,
						Size = player.InDeck.Count()
					},
					Secrets = SortedDbfIds(player.InSecret),
					Hero = DbfId(Find(player, HeroId(game.LocalPlayerEntity))),
					Hand = new BoardStateHand
					{
						Cards = SortedDbfIds(player.InHand),
						Size = player.InHand.Count()
					},
					HeroPower = DbfId(FindHeroPower(player)),
					Weapon = DbfId(Find(player, WeaponId(game.LocalPlayerEntity))),
					Quest = Quest(player.InQuest.FirstOrDefault()),
					Fatigue = game.LocalPlayerEntity.GetTag(GameTag.FATIGUE)
				},
				Opponent = new BoardStatePlayer
				{
					Board = SortedDbfIds(opponent.InPlay.Where(x => x.IsMinion)),
					Deck = new BoardStateDeck
					{
						Size = opponent.InDeck.Count()
					},
					Hand = new BoardStateHand
					{
						Size = opponent.InHand.Count()
					},
					Hero = DbfId(Find(opponent, HeroId(game.OpposingPlayerEntity))),
					HeroPower = DbfId(FindHeroPower(opponent)),
					Weapon = DbfId(Find(opponent, WeaponId(game.OpposingPlayerEntity))),
					Quest = Quest(opponent.InQuest.FirstOrDefault()),
					Fatigue = game.OpposingPlayerEntity.GetTag(GameTag.FATIGUE)
				},
			};
		}
		
		public static readonly Dictionary<CardClass, string> HeroCardId = new Dictionary<CardClass, string>
		{
			{CardClass.WARRIOR, CardIds.Collectible.Warrior.GarroshHellscream},
			{CardClass.SHAMAN, CardIds.Collectible.Shaman.Thrall},
			{CardClass.ROGUE, CardIds.Collectible.Rogue.ValeeraSanguinar},
			{CardClass.PALADIN, CardIds.Collectible.Paladin.UtherLightbringer},
			{CardClass.HUNTER, CardIds.Collectible.Hunter.Rexxar},
			{CardClass.DRUID, CardIds.Collectible.Druid.MalfurionStormrage},
			{CardClass.WARLOCK, CardIds.Collectible.Warlock.Guldan},
			{CardClass.MAGE, CardIds.Collectible.Mage.JainaProudmoore},
			{CardClass.PRIEST, CardIds.Collectible.Priest.AnduinWrynn}
		};

	}
}
