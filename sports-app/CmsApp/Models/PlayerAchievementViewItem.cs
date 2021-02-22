using System;
using System.Collections.Generic;
using DataService.DTO;
using AppModel;
using DataService;

namespace CmsApp.Models
{
    public class PlayerAchievementsViewModel
    {
        public CultEnum Culture { get; set; }

        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public int TeamId { get; set; }
        public int PlayerId { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
        public string SectionAlias { get; set; } 
        public List<AchievementsBySeason> AchievementsBySeasonList { get; set; }

        public bool IsEditAllowed { get; set; }
        public bool IsBasketball { get; internal set; }
        public IDictionary<Season,List<StatisticsDTO>> PlayersStatistic { get; set; }
        public bool HasActiveCompetitions { get; internal set; }
        public List<CompetitionAchievementBySeason> CompetitionsList { get; internal set; }
        public bool IsTennis { get; internal set; }
        public IEnumerable<TennisLeagueGameForm> TennisCompetitionsGames { get; internal set; }
        public bool IsMartialArts { get; internal set; }
        public IEnumerable<MartialArtsStatsBySeason> MartialArtsStatsBySeason { get; set; }
        public IEnumerable<TennisRank> PointsAndRanks { get; internal set; }
        public string PlayerName { get; set; }
        public string BirthDay { get; set; }
        public string IdentNum { get; set; }
        public int? AthleteNumber { get; set; }
        public string PlayerClub { get; set; }
    }

    public class PlayerAchievementViewItem
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int RankId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DateCompleted { get; set; }
        public int Score { get; set; }

        public SportRankViewModel SportRank { get; set; }
    }

    public class AchievementsBySeason
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<PlayerAchievementViewItem> Achievements { get; set; }
    }

    public class MartialArtsStatsBySeason
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<MartialArtsCompetitionDto> MartialArtsStats { get; set; }
    }

}