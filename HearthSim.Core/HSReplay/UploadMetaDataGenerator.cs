using System;
using System.Linq;
using HearthDb.Enums;
using HearthSim.Core.Hearthstone;
using HSReplay;

namespace HearthSim.Core.HSReplay
{
	internal static class UploadMetaDataGenerator
	{
		public static UploadMetaData Generate(int? build, GameState game, int wins, int losses)
		{
			var metaData = new UploadMetaData();
			var playerInfo = GetPlayerInfo(game, wins, losses);
			if(playerInfo != null)
			{
				metaData.Player1 = playerInfo.Player1;
				metaData.Player2 = playerInfo.Player2;
			}
			if(game?.ServerInfo != null)
			{
				metaData.ServerIp = game.ServerInfo.Address;
				metaData.ServerPort = game.ServerInfo.Port.ToString();
				metaData.GameHandle = game.ServerInfo.GameHandle.ToString();
				metaData.ClientHandle = game.ServerInfo.ClientHandle.ToString();
				metaData.SpectatePassword = game.ServerInfo.SpectatorPassword;
				metaData.AuroraPassword = game.ServerInfo.AuroraPassword;
				metaData.ServerVersion = game.ServerInfo.Version;
				metaData.Resumable = game.ServerInfo.Resumable;
				if(game.ServerInfo.Mission > 0)
					metaData.ScenarioId = game.ServerInfo.Mission;
			}
			if(game?.CreatedAt > DateTime.MinValue)
				metaData.MatchStart = game.CreatedAt.ToString("o");
			if(game?.MatchInfo != null)
			{
				metaData.GameType = (int?)Converters.GetBnetGameType((GameType)game.MatchInfo.GameType, (FormatType)game.MatchInfo.FormatType);
				metaData.Format = game.MatchInfo.FormatType;
				metaData.SpectatorMode = game.MatchInfo.Spectator;
				metaData.BrawlSeason = game.MatchInfo.BrawlSeasonId;
				metaData.LadderSeason = game.MatchInfo.RankedSeasonId;
			}
			metaData.FriendlyPlayerId = game?.LocalPlayer?.PlayerId;
			metaData.HearthstoneBuild = build;
			return metaData;
		}

		private static PlayerInfo GetPlayerInfo(GameState game, int wins, int losses)
		{
			var localPlayer = game?.MatchInfo?.LocalPlayer;
			var opposingPlayer = game?.MatchInfo?.OpposingPlayer;
			if(localPlayer == null || opposingPlayer == null)
				return null;

			var friendly = new UploadMetaData.Player();
			var opposing = new UploadMetaData.Player();

			var standard = game.MatchInfo.FormatType == (int)FormatType.FT_STANDARD;

			var rank = standard ? localPlayer.StandardRank : localPlayer.WildRank;
			if(rank > 0)
				friendly.Rank = rank;

			var legendRank = standard ? localPlayer.StandardLegendRank : localPlayer.WildLegendRank;
			if(legendRank > 0)
				friendly.LegendRank = legendRank;

			if(localPlayer.CardBackId > 0)
				friendly.Cardback = localPlayer.CardBackId;

			var stars = standard ? localPlayer.StandardStars : localPlayer.WildStars;
			if(stars > 0)
				friendly.Stars = stars;

			if(game.LocalPlayer.Deck?.Cards.Sum(x => x.Count) == 30)
			{
				friendly.DeckList = game.LocalPlayer.Deck.Cards.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToArray();
				if(game.LocalPlayer.Deck.DeckId > 0)
					friendly.DeckId = game.LocalPlayer.Deck.DeckId;
			}

			if(game.MatchInfo.GameType == (int)GameType.GT_ARENA || IsBrawl((GameType)game.MatchInfo.GameType))
			{
				if(wins > 0)
					friendly.Wins = wins;
				if(losses > 0)
					friendly.Losses = losses;
			}

			var oppRank = standard ? opposingPlayer.StandardRank : opposingPlayer.WildRank;
			if(oppRank > 0)
				opposing.Rank = oppRank;

			var oppLegendRank = standard ? opposingPlayer.StandardLegendRank : opposingPlayer.WildLegendRank;
			if(oppLegendRank > 0)
				opposing.LegendRank = oppLegendRank;

			if(opposingPlayer.CardBackId > 0)
				opposing.Cardback = opposingPlayer.CardBackId;

			if(game.OpposingPlayer.Deck?.Cards.Sum(x => x.Count) == 30)
			{
				opposing.DeckList = game.OpposingPlayer.Deck.Cards.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToArray();
				if(game.OpposingPlayer.Deck.DeckId > 0)
					opposing.DeckId = game.OpposingPlayer.Deck.DeckId;
			}

			return new PlayerInfo(localPlayer.Id == 1 ? friendly : opposing, localPlayer.Id == 2 ? friendly : opposing);
		}

		private static bool IsBrawl(GameType gameType) => new[] {GameType.GT_TAVERNBRAWL, GameType.GT_TB_1P_VS_AI, GameType.GT_TB_2P_COOP}.Contains(gameType);
	}

	public class PlayerInfo
	{
		public UploadMetaData.Player Player1 { get; }
		public UploadMetaData.Player Player2 { get; }
		public int FriendlyPlayerId { get; }
		public PlayerInfo(UploadMetaData.Player player1, UploadMetaData.Player player2, int friendlyPlayerId = -1)
		{
			Player1 = player1;
			Player2 = player2;
			FriendlyPlayerId = friendlyPlayerId;
		}
	}
}
